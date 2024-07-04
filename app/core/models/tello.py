import socket
import time
import threading
import datetime
import queue

class Tello:
    def __init__(self, drone_id, drone_ip, cmd_port, state_port, video_port) -> None:

        self.drone_id = drone_id
        self.drone_ip = drone_ip
        self.cmd_port, self.state_port, self.video_port = cmd_port, state_port, video_port
        
        # Set Cmd Socket -> UDP
        self.cmd_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.cmd_socket.bind(('', cmd_port))
        
        # Set State Socket -> UDP
        self.state_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.state_socket.bind(('', state_port))
        
        self.cmd_queue, self.cmd_event = queue.Queue(), threading.Event()
        
        
        self.cmd_max_time_out = 15
        self.cmd_max_retry = 2
        
        self.cmd_buffer_size = 1024
         
        self.thread_start()
    
    def command(self, cmd):
        print(f"Command Line : {cmd}")
        self.cmd_queue.put(cmd)
        
    def sender(self):
        self.cmd_event.set()        # allow the first wait to proceed
        while True:
            self.cmd_event.wait()   # block second queue.get() until an event is set from receiver or failure set
            self.cmd_event.clear()  # block a timeout-enabled waiting
            cmd = self.cmd_queue.get()
            
            self.cmd_socket.sendto(cmd.encode('utf-8'), (self.drone_ip, self.cmd_port))
            cmd_ok = False
            
            # 명령을 보낸 후 Tello에서 응답이 올 때까지 대기
            for i in range(self.cmd_max_retry):
                if self.cmd_event.wait(timeout=self.cmd_max_time_out):
                    cmd_ok = True
                    break
                
                else:
                    print(f' # Sender : Failed to send command: "{cmd}", Failure sequence: {i+1}.')
                    self.cmd_socket.sendto(cmd.encode('utf-8'), (self.drone_ip, self.cmd_port))
                    
                    
            if cmd_ok:
                print(f' # Sender : Success To Control "{cmd}".')
                
            else:
                self.cmd_queue = queue.Queue() # Queue 객체 초기화
                self.cmd_event.set() # The failure set
                print(f' # Sender : Stop retry: "{cmd}", Maximum re-tries: {self.cmd_max_retry}.')        
        
        
    def receiver(self):
        while True:
            bytes_, address = self.cmd_socket.recvfrom(self.cmd_buffer_size)
            
            if bytes_ == b'ok':
                self.cmd_event.set() # one command has been successfully executed. Begin new execution.
                
            else:
                print(f' $ Receiver : From Tello :: {bytes_.encode()}')    
  
    def update_state(self):     
        while True:
            bytes_, address = self.state_socket.recvfrom(1024)
            str_ = bytes_.decode()
            
            print(f" % \033[33m{self.drone_id}\033[0m State Updater : From Tello :: {str_}")
    
    def thread_start(self):
        print(f" @ Tello Model : Receiver Thread start.")
        threading.Thread(target=self.receiver, daemon=True).start()
        # threading.Thread(target=self.receiver).start()
        print(f" @ Tello Model : Sender Thread start.")
        threading.Thread(target=self.sender, daemon=True).start()
        # threading.Thread(target=self.sender).start()
        print(f" @ Tello Model : Update State Thread start.")
        threading.Thread(target=self.update_state , daemon=True).start()
        # threading.Thread(target=self.update_state).start()
        
        
            
        
    
        
        