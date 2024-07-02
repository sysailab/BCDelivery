import sys
from ip_table import IPTable
import json
from tello import Tello
import time
import threading
# from send_command import send_command


if __name__ == "__main__":
    
    ip_table_dict = open("./drone_ip_table.json", 'r')
    ip_table_dict = json.load(ip_table_dict)
    
    WIFI_SSID = '612-drone'      # 공통 Wi-Fi 네트워크의 SSID
    WIFI_PASSWORD = 'a-12345678'  # 공통 Wi-Fi 네트워크의 비밀번호
    
    drone_id = sys.argv[1]
    # cmd_option = sys.argv[2]
    
    TELLO_IP = ip_table_dict[drone_id]["ip_address"]
    TELLO_CMD_PORT = ip_table_dict[drone_id]["ports"]["command"]
    TELLO_STATE_PORT = ip_table_dict[drone_id]["ports"]["state"]
    TELLO_VIDEO_PORT = ip_table_dict[drone_id]["ports"]["video"]
    
    
    tello = Tello(TELLO_IP, TELLO_CMD_PORT, TELLO_STATE_PORT, TELLO_VIDEO_PORT)
    
    print("Tello Init Succesful")
    
    # while True:
    #     print("뭘 치고 싶음?")
    #     a = input()
    #     print(a)
        
    
    # tello.command("command")
    
    # tello.command("takeoff")
    
    # time.sleep(3)
    
    # tello.command("land")
    
    # time.sleep(1)
    
    # tello.command("command")
    
    # threading.Thread.join()
    # if cmd_option == "wifi":
    #     set_wifi(TELLO_IP, TELLO_CMD_PORT, WIFI_SSID, WIFI_PASSWORD)
    
    # elif cmd_option == "station":
    #     set_station_mode(TELLO_IP, TELLO_CMD_PORT, WIFI_SSID, WIFI_PASSWORD)
        
    # elif cmd_option == "command":
    # send_command("command", TELLO_IP, TELLO_CMD_PORT)
    # else:
    #     print(f" # {cmd_option} : No Command Found. Process Will Stop.")
        