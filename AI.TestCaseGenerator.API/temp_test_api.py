import json, os, tempfile, uuid, requests

base = 'http://localhost:5000'


def request(method, path, headers=None, json_body=None, files=None, data=None, timeout=30):
    try:
        resp = requests.request(method, f'{base}{path}', headers=headers, json=json_body, files=files, data=data, timeout=timeout)
        return resp.status_code, resp.text
    except Exception as e:
        return 0, str(e)

# Register or login
email = 'endpointtester6@example.com'
reg_status, reg_text = request('POST', '/api/Auth/register', headers={'Content-Type':'application/json'}, json_body={
    'fullName':'Endpoint Tester',
    'email':email,
    'password':'TestPass123!',
    'confirmPassword':'TestPass123!'
})
if reg_status == 201:
    token = json.loads(reg_text)['token']
else:
    login_status, login_text = request('POST', '/api/Auth/login', headers={'Content-Type':'application/json'}, json_body={
        'email': email,
        'password':'TestPass123!'
    })
    token = json.loads(login_text)['token']

headers = {'Authorization': f'Bearer {token}', 'Content-Type': 'application/json'}

results = []

# helper

def add_result(name, status, body):
    results.append({'name': name, 'status': status, 'body': body[:500].replace('\n', ' ').replace('\r', ' ')})

# health and auth
status, body = request('GET', '/api/Health')
add_result('Health', status, body)
status, body = request('GET', '/api/Auth/profile', headers=headers)
add_result('Auth Profile', status, body)
status, body = request('GET', '/api/Auth/validate', headers=headers)
add_result('Auth Validate', status, body)

# project create/list/get/update/delete
status, body = request('GET', '/api/Project', headers=headers)
add_result('Project List', status, body)
status, body = request('POST', '/api/Project', headers=headers, json_body={'name':'API Test Project','description':'Created during validation'})
add_result('Project Create', status, body)
project_id = 1
try:
    project_id = json.loads(body)['id']
except Exception:
    pass
status, body = request('GET', f'/api/Project/{project_id}', headers=headers)
add_result('Project Get', status, body)
status, body = request('PUT', f'/api/Project/{project_id}', headers=headers, json_body={'name':'Updated Project','description':'Updated during validation'})
add_result('Project Update', status, body)
status, body = request('DELETE', f'/api/Project/{project_id}', headers=headers)
add_result('Project Delete', status, body)

# Recreate project for downstream tests
status, body = request('POST', '/api/Project', headers=headers, json_body={'name':'API Test Project','description':'Created during validation'})
add_result('Project Recreate', status, body)
project_id = 1
try:
    project_id = json.loads(body)['id']
except Exception:
    pass

# document upload/list/download/process
with tempfile.NamedTemporaryFile('w', suffix='.txt', delete=False) as f:
    f.write('sample upload content')
    temp_path = f.name
try:
    with open(temp_path, 'rb') as f:
        files = {'File': ('sample.txt', f, 'text/plain')}
        data = {'ProjectId': str(project_id)}
        status, body = request('POST', '/api/Document/upload', headers={'Authorization': f'Bearer {token}'}, files=files, data=data)
    add_result('Document Upload', status, body)
finally:
    os.remove(temp_path)

status, body = request('GET', f'/api/Document/project/{project_id}', headers=headers)
add_result('Document Project', status, body)
status, body = request('GET', '/api/Document/1', headers=headers)
add_result('Document Detail', status, body)
status, body = request('GET', '/api/Document/download/1', headers=headers)
add_result('Document Download', status, body)
status, body = request('DELETE', '/api/Document/1', headers=headers)
add_result('Document Delete', status, body)
status, body = request('POST', '/api/Document/process/1', headers=headers)
add_result('Document Process', status, body)

# test case routes
status, body = request('POST', '/api/TestCase/generate', headers=headers, json_body={
    'projectId': project_id,
    'moduleName': 'Login',
    'testType': 'Positive',
    'numberOfTestCases': 2,
    'prompt': 'Generate sample test cases'
})
add_result('TestCase Generate', status, body)
status, body = request('GET', f'/api/TestCase/project/{project_id}', headers=headers)
add_result('TestCase Project', status, body)
status, body = request('GET', '/api/TestCase/1', headers=headers)
add_result('TestCase Detail', status, body)
status, body = request('GET', f'/api/TestCase/search?projectId={project_id}&keyword=login', headers=headers)
add_result('TestCase Search', status, body)
status, body = request('GET', f'/api/TestCase/filter?projectId={project_id}&priority=High&testType=Positive', headers=headers)
add_result('TestCase Filter', status, body)

# ai chat routes
status, body = request('POST', '/api/AIChat/ask', headers=headers, json_body={'projectId': project_id, 'question': 'Summarize this project'})
add_result('AIChat Ask', status, body)
status, body = request('GET', f'/api/AIChat/history/{project_id}', headers=headers)
add_result('AIChat History', status, body)
status, body = request('DELETE', f'/api/AIChat/history/{project_id}', headers=headers)
add_result('AIChat Delete', status, body)
status, body = request('POST', '/api/AIChat/regenerate', headers=headers, json_body={'projectId': project_id, 'question': 'Summarize this project'})
add_result('AIChat Regenerate', status, body)

# export routes
for path in ['/api/Export/excel/1', '/api/Export/pdf/1', '/api/Export/pdf/testcase/1']:
    status, body = request('GET', path, headers=headers)
    add_result(path, status, body)

# user routes
status, body = request('GET', '/api/User/profile', headers=headers)
add_result('User Profile', status, body)
status, body = request('PUT', '/api/User/profile', headers=headers, json_body={'fullName':'Updated Name'})
add_result('User Update', status, body)
status, body = request('PUT', '/api/User/change-password', headers=headers, json_body={'currentPassword':'TestPass123!','newPassword':'NewPass123!'})
add_result('User Change Password', status, body)
status, body = request('DELETE', '/api/User', headers=headers)
add_result('User Delete', status, body)

# logout
status, body = request('POST', '/api/Auth/logout', headers=headers)
add_result('Auth Logout', status, body)

for item in results:
    print(f"{item['name']}|{item['status']}|{item['body']}")
