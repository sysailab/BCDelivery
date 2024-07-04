# BCDelivery


File Structure
```
📦BCDelivery
 ┣ 📂app
 ┃ ┣ 📂core
 ┃ ┃ ┣ 📂models
 ┃ ┃ ┃ ┣ 📂sim_models
 ┃ ┃ ┃ ┃ ┗ 📜test_model.py
 ┃ ┃ ┃ ┣ 📜base_model.py
 ┃ ┃ ┃ ┣ 📜simulator.py
 ┃ ┃ ┃ ┗ 📜tello.py
 ┃ ┃ ┗ 📂routers
 ┃ ┃ ┃ ┣ 📜drone.py
 ┃ ┃ ┃ ┣ 📜drone_ip_table.json
 ┃ ┃ ┃ ┗ 📜sim.py
 ┃ ┣ 📂static
 ┃ ┗ 📜fastapi.py
 ┣ 📂DroneCommunicator
 ┃ ┣ 📜drone_ip_table.json
 ┃ ┣ 📜ip_table.py
 ┃ ┗ 📜main.py
 ┣ 📂instance
 ┃ ┗ 📜config.py
 ┣ 📜.gitignore
 ┣ 📜config.py
 ┣ 📜main.py
 ┣ 📜README.md
 ┣ 📜requirements.txt
 ┗ 📜Tello Stream.jpg
```

./instance/config.py 생성 후, 아래와 같이 코드 생성 (예시)

```python
HOST = "0.0.0.0"
PORT = 17148
RELOAD = True
```

실행 방법
```bash
python3 main.py
```

Endpoints

swagger
```http
http://localhost:17148/docs
```

