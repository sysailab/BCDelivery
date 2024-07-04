from pyevsim import BehaviorModelExecutor, SystemSimulator, Infinite
from .sim_models.test_model import TestModel
import threading

class Simulator():
    def __init__(self, _recv_queue= None, _send_queue= None, sm_event= None) -> None:        
        self.engine_name = "Sim"
        
        self._recv_queue = _recv_queue
        self._send_queue = _send_queue
        self.sm_event = sm_event
        
        ss = SystemSimulator()
        ss.register_engine(self.engine_name, "VIRTUAL_TIME", 1)
        
        self.sm_engine = ss.get_engine(self.engine_name)
        
        # Set Engine Port
        self.engine_init_port()
        
        # Set Engine entity
        self.engine_register_entity()

        # Set Model's Relation
        self.engine_coupling_relation()
        
        # self.engine_start()

    def engine_init_port(self) -> None:
        """
        시뮬레이션 엔진 입출력 포트 설정
        """
        
        # self.engine.insert_input_port("start")
        # self.engine.insert_input_port("init")
        # self.engine.insert_input_port("pose_recv")
        # self.engine.insert_input_port("send_result")
        
        # self.engine.insert_output_port("pose_check")   
        # self.engine.insert_output_port("pose_classify")   
        # self.engine.insert_output_port("pose_recv")   
        # self.engine.insert_output_port("send_result")    
        # self.engine.insert_output_port("end")   
        
        pass
        
    def engine_register_entity(self) -> None:
        """
        시뮬레이션 엔진에 등록할 모델 설정
        """
    #     self.socket_model = SocketModel(instance_time = 0, destruct_time = Infinite,\
    #         name = "socket_model", engine_name = "armleg")
        
    #     self.pose_check_model = PoseCheckModel(instance_time = 0, destruct_time = Infinite,\
    #         name = "pose_check_model", engine_name = "armleg")
        
    #     self.pose_classify_model = PoseClassifyModel(instance_time = 0, destruct_time = Infinite,\
    #         name = "pose_classify_model", engine_name = "armleg")
        
        self.test_model = TestModel(instance_time = 0, destruct_time = Infinite,\
            name = "test_model", engine_name = self.engine_name, \
                _recv_queue=self._recv_queue, _send_queue = self._send_queue, event= self.sm_event)
        
        
    #     self.engine.register_entity(self.socket_model)
    #     self.engine.register_entity(self.pose_check_model)
    #     self.engine.register_entity(self.pose_classify_model)
        self.sm_engine.register_entity(self.test_model)
    

    def engine_coupling_relation(self) -> None:
        """
        시뮬레이션 엔진 내의 모델 간의 상호작용 설정
        """        
        # self.engine.coupling_relation(self.socket_model, "pose_check", self.pose_check_model, "start")
        # self.engine.coupling_relation(self.pose_check_model, "pose_classify", self.pose_classify_model, "start")
        # self.engine.coupling_relation(self.pose_classify_model, "pose_recv", self.socket_model, "pose_recv")
        # self.engine.coupling_relation(self.pose_classify_model, "send_result", self.socket_model, "send_result")
        # self.engine.coupling_relation(self.pose_classify_model, "end", self.socket_model, "init")        
        pass
    
    def engine_start(self) -> None:
        # self.engine.insert_external_event("start","start")
        print(" * Simulation Engine Start Succesfully.")
        
        # 쓰레드간 Signal 충돌이 나므로 _tm을 False로 설정해야 Main Thread가 아닌 상태여도 시뮬레이션 엔진이 동작함
        self.sm_engine.simulate(_tm = False)
        
    def engine_thread_start(self) -> None:
        print("Sim Thread Start")
        threading.Thread(target=self.engine_start, daemon=True).start()
        
if __name__ == '__main__':
    Simulator().engine_thread_start()