from pyevsim import BehaviorModelExecutor, Infinite, SysMessage
import time

# 파이썬 파일명이랑 클래스 이름이랑 똑같이!
class TestModel(BehaviorModelExecutor):
    def __init__(self, instance_time, destruct_time, name, engine_name, _recv_queue, _send_queue, event):
        BehaviorModelExecutor.__init__(self, instance_time, destruct_time, name, engine_name)    
        
        self._recv_queue = _recv_queue
        self._send_queue = _send_queue
        self._event = event
        
        # State
        self.set_state()
        
        # Port
        self.set_port()
        
    def set_state(self):
        """ 시뮬레이션 모델의 상태(State) 설정
        """
        self.insert_state("IDLE", Infinite)
        self.insert_state("GENERATE", 1)
        
        self.init_state("GENERATE")
    
    def set_port(self):
        """ 시뮬레이션 모델의 입출력 포트(Port) 설정
        """
        self.insert_input_port("start")
        self.insert_input_port("init")
        
        self.insert_input_port("count")
        self.insert_input_port("cycle_count")
        
        self.insert_input_port("pose_recv")
        
        self.insert_output_port("pose_check")
        
        
    def ext_trans(self, port, msg):
        # if port == "start":
        #     self._cur_state = "RECVING"
            
        # elif port == "pose_recv":
        #     self.counter = msg.retrieve()[0]
        #     print("현재 횟수 : ", self.counter)
        #     self._cur_state = "RECVING"
        
        # elif port == "send_result":
        #     self.counter = msg.retrieve()[0]
        #     print("데이터 전송 : ", self.counter)
        
        pass
        
    def output(self):
        if self._cur_state == "GENERATE":
            state_req_data = self._recv_queue.get()

            
            """
            이 부분에서 시뮬레이션 연산 코드 짜면 댐.
            위 아래 queue, event 관련된 코드는 건드리면 망가짐순 ㅠ 살살 다뤄줘
            """
            
            
            # state_req_data.home += 1
            # state_req_data.store += 1
            
            self._send_queue.put(state_req_data)
            self._event.set()
    
    def int_trans(self):
        pass  