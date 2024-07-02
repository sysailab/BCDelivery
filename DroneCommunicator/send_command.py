import socket
import time
from tello import Tello

# tello = Tello()

# class Tello_Command(Tello):
#     def __init__(self, drone_ip, cmd_port, state_port, video_port) -> None:
#         super().__init__(drone_ip, cmd_port, state_port, video_port)
        
#         pass

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(('', 8889))






def send_command(command, ip, port):
    """드론에 명령을 전송하는 함수"""
    sock.sendto(command.encode(), (ip, port))
    response, _ = sock.recvfrom(1024)
    print(f"   * Response From TT : {response.decode()}")
    
    time.sleep(1)
    return response.decode()
    


def set_station_mode(tello_ip, tello_port, ssid, password):
    """스테이션 모드를 설정하는 함수"""
    # command mode 활성화
    print(f" # Try To Activate SDK Mode")
    response = send_command("command", tello_ip, tello_port)

    # 스테이션 모드 설정 명령어 전송
    station_mode_command = f"ap {ssid} {password}"
    print(f" # Try To Activate Station Mode")
    response = send_command(station_mode_command, tello_ip, tello_port)
    # print(f"Station mode command response: {response}")


def set_wifi(tello_ip, tello_port, ssid, password):
    """Wifi 이름을 설정하는 함수"""
    
    ssid = "12345678"
    password = "12345678"
    
    wifi_set_cmd = f"wifi {ssid} {password}"
    
    print(f" # Try To Activate SDK Mode")
    response = send_command("command", tello_ip, tello_port)    
    
    
    # command mode 활성화
    print(f" # Try To Set Wifi")

    response = send_command(wifi_set_cmd, tello_ip, tello_port)
    print(f"Wifi mode command response: {response}")
