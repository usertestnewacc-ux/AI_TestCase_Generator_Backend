import os
import sys
import requests

base = 'http://localhost:5276'
email = 'endpointtester6@example.com'
password = 'TestPass123!'

try:
    print('Login...')
    r = requests.post(f'{base}/api/Auth/login', json={'email': email, 'password': password}, timeout=30)
    print('login', r.status_code, r.text)
    if r.status_code != 200:
        print('Registering...')
        r2 = requests.post(
            f'{base}/api/Auth/register',
            json={'fullName': 'Endpoint Tester', 'email': email, 'password': password, 'confirmPassword': password},
            timeout=30)
        print('register', r2.status_code, r2.text)
        if r2.status_code not in (200, 201):
            sys.exit(1)
        r = r2

    data = r.json()
    token = data.get('token')
    print('token', bool(token))
    if not token:
        print('No token returned; exiting')
        sys.exit(1)

    headers = {'Authorization': f'Bearer {token}'}
    r3 = requests.get(f'{base}/api/Project', headers=headers, timeout=30)
    print('projects', r3.status_code, r3.text)

    if r3.status_code == 200 and r3.json():
        proj_id = r3.json()[0]['id']
    else:
        r4 = requests.post(
            f'{base}/api/Project',
            headers=headers,
            json={'name': 'API Test Project', 'description': 'Temporary project for PDF test'},
            timeout=30)
        print('create project', r4.status_code, r4.text)
        if r4.status_code not in (200, 201):
            sys.exit(1)
        proj_id = r4.json()['id']

    print('project id', proj_id)
    uploads_dir = os.path.join(os.getcwd(), 'Uploads')
    pdfs = [f for f in os.listdir(uploads_dir) if f.lower().endswith('.pdf')]
    print('pdfs', pdfs)
    if not pdfs:
        print('No PDF files found in Uploads directory.')
        sys.exit(1)

    pdf_path = os.path.join(uploads_dir, pdfs[0])
    print('Using PDF', pdf_path)

    with open(pdf_path, 'rb') as pdf_file:
        files = {'File': ('file.pdf', pdf_file, 'application/pdf')}
        data = {'ProjectId': str(proj_id)}
        r5 = requests.post(f'{base}/api/Document/upload', headers=headers, files=files, data=data, timeout=120)
        print('upload', r5.status_code, r5.text)
        if r5.status_code not in (200, 201):
            sys.exit(1)
        doc_id = r5.json().get('id')
        print('doc_id', doc_id)

    r6 = requests.post(f'{base}/api/Document/process/{doc_id}', headers=headers, json={}, timeout=300)
    print('process', r6.status_code, r6.text)
    r7 = requests.get(f'{base}/api/Document/{doc_id}', headers=headers, timeout=30)
    print('detail', r7.status_code, r7.text)
except Exception as e:
    print('ERROR', repr(e))
