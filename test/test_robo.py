import socket

# Robomaster EP의 IP 주소와 포트 번호를 설정합니다.
ROBO_IP = '192.168.50.39'  # Robomaster EP의 기본 IP 주소
ROBO_PORT = 20020        # Robomaster EP의 기본 포트 번호

# UDP
r_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print("Try to connet")
r_socket.connect((ROBO_IP, ROBO_PORT))
print("Connect Fin")



# r_socket.sendall("hello".encode())

# data = r_socket.recv(1024)
# print(data)