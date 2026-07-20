import json
import mimetypes
import os
import sys
import uuid
import urllib.error
import urllib.request

BASE_URL = 'http://127.0.0.1:5276'
LOGIN_PAYLOAD = {
    'email': 'endpointtester@example.com',
    'password': 'TestPass123!'
}
PROJECT_PAYLOAD = {
    'name': 'Endpoint Validation Project',
    'description': 'Project created for endpoint validation.'
}
DOC_PATH = os.path.abspath(os.path.join('..', '..', 'AI_Test_Case_Generator_SRS.docx'))


def encode_multipart_formdata(fields, files):
    boundary = '----WebKitFormBoundary' + uuid.uuid4().hex
    body = bytearray()
    for name, value in fields.items():
        body.extend(f'--{boundary}\r\n'.encode('utf-8'))
        body.extend(f'Content-Disposition: form-data; name="{name}"\r\n\r\n'.encode('utf-8'))
        body.extend(value.encode('utf-8'))
        body.extend(b'\r\n')

    for name, filename, filedata, content_type in files:
        body.extend(f'--{boundary}\r\n'.encode('utf-8'))
        body.extend(f'Content-Disposition: form-data; name="{name}"; filename="{filename}"\r\n'.encode('utf-8'))
        body.extend(f'Content-Type: {content_type}\r\n\r\n'.encode('utf-8'))
        body.extend(filedata)
        body.extend(b'\r\n')

    body.extend(f'--{boundary}--\r\n'.encode('utf-8'))
    content_type = f'multipart/form-data; boundary={boundary}'
    return content_type, bytes(body)


def request_json(path, method='GET', data=None, headers=None):
    if headers is None:
        headers = {}
    url = BASE_URL + path
    if data is not None and isinstance(data, dict):
        data = json.dumps(data).encode('utf-8')
        headers['Content-Type'] = 'application/json'
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            return resp.status, json.loads(resp.read().decode('utf-8'))
    except urllib.error.HTTPError as e:
        body = e.read().decode('utf-8', errors='replace')
        return e.code, {'error': body}
    except Exception as exc:
        return None, {'exception': str(exc)}


def main():
    print('LOGIN')
    status, resp = request_json('/api/Auth/login', method='POST', data=LOGIN_PAYLOAD)
    print(status, json.dumps(resp, indent=2))
    if status != 200 or 'token' not in resp:
        raise SystemExit('Login failed')
    token = resp['token']
    headers = {'Authorization': f'Bearer {token}'}

    print('\nCHECK AUTH VALIDATION')
    status, resp = request_json('/api/Auth/validate', headers=headers)
    print(status, json.dumps(resp, indent=2))
    if status != 200:
        raise SystemExit('Token validation failed')

    print('\nGET PROJECTS')
    status, resp = request_json('/api/project', headers=headers)
    print(status, json.dumps(resp, indent=2))
    project_id = None
    if status == 200 and isinstance(resp, list) and len(resp) > 0:
        project_id = resp[0]['id']
    else:
        print('CREATING PROJECT')
        status, resp = request_json('/api/project', method='POST', data=PROJECT_PAYLOAD, headers=headers)
        print(status, json.dumps(resp, indent=2))
        if status != 201:
            raise SystemExit('Project creation failed')
        project_id = resp['id']

    print('\nPROJECT ID:', project_id)
    if not os.path.exists(DOC_PATH):
        raise SystemExit(f'Document path not found: {DOC_PATH}')

    print('\nUPLOAD DOCUMENT')
    with open(DOC_PATH, 'rb') as f:
        filedata = f.read()
    content_type, body = encode_multipart_formdata(
        {'ProjectId': str(project_id)},
        [('File', os.path.basename(DOC_PATH), filedata, mimetypes.guess_type(DOC_PATH)[0] or 'application/octet-stream')]
    )
    headers_upload = {'Authorization': f'Bearer {token}', 'Content-Type': content_type}
    req = urllib.request.Request(BASE_URL + '/api/Document/upload', data=body, headers=headers_upload, method='POST')
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            upload_resp = json.loads(resp.read().decode('utf-8'))
            print(resp.status, json.dumps(upload_resp, indent=2))
    except urllib.error.HTTPError as e:
        print('UPLOAD FAILED', e.code)
        print(e.read().decode('utf-8', errors='replace'))
        raise
    except Exception as exc:
        raise

    document_id = upload_resp.get('id')
    if not document_id:
        raise SystemExit('Upload did not return document id')

    print('\nPROCESS DOCUMENT')
    status, resp = request_json(f'/api/Document/process/{document_id}', method='POST', headers=headers)
    print(status, json.dumps(resp, indent=2))
    if status != 200:
        raise SystemExit('Document processing failed')

    print('\nGENERATE TEST CASES')
    generate_payload = {
        'ProjectId': project_id,
        'ModuleName': 'Login',
        'TestType': 'All',
        'NumberOfTestCases': 5,
        'Prompt': 'Generate test cases for this project.'
    }
    status, resp = request_json('/api/TestCase/generate', method='POST', data=generate_payload, headers=headers)
    print(status, json.dumps(resp, indent=2))
    if status != 200:
        raise SystemExit('Test case generation failed')

    print('\nAI CHAT ASK')
    chat_payload = {'ProjectId': project_id, 'Question': 'What is this project about?'}
    status, resp = request_json('/api/AIChat/ask', method='POST', data=chat_payload, headers=headers)
    print(status, json.dumps(resp, indent=2))
    if status != 200:
        raise SystemExit('AI chat failed')


if __name__ == '__main__':
    main()
