from abc import ABC, abstractmethod
import asyncio
import threading
from robomaster import robot, camera, conn

class BaseRobot(ABC):
    def __init__(self) -> None:
        super().__init__()
        
        self.cmd_queue = asyncio.Queue()
        # self.video_queue = asyncio.Queue(maxsize=1)
        # self.video_queue = queue.Queue(maxsize=1)
        self.rep_queue = asyncio.Queue()
        self.async_event = asyncio.Event()
        self.async_tasks = []

    # def __del__(self):
    #     print("Mother")
    
    def __del__(self):
        super().__del__()
        print("Mother")
        
        
        
    @abstractmethod
    async def initialize(self):
        pass
        
    @abstractmethod
    def command(self, cmd):
        pass
    
    @abstractmethod
    async def sender(self):
        pass
    
    @abstractmethod
    async def receiver(self):
        pass
    
    @abstractmethod
    async def update_state(self):
        pass
    
    @abstractmethod
    def video_stream(self):
        pass
    
    @abstractmethod
    def coroutine_start(self):
        pass

CMD_CHASSIS_X_UP = "w"
CMD_CHASSIS_X_DOWN = "s"
CMD_CHASSIS_Y_UP = "d"
CMD_CHASSIS_Y_DOWN = "a"

CMD_GIMBAL = "gimbal"

class RoboEP(BaseRobot):
    def __init__(self, sn:str, ip:str = None) -> None:
        super().__init__()
        
        self.sn = sn
        self.ep_robot = None
        self.ep_chassis = None
        self.ep_gimbal = None
        self.ep_camera = None        
        
        self.distance = None
        self.image = None
        
        self.is_stream = False
        self.is_running = True
            
        # self._finalizer = weakref.finalize(self, self._cleanup)
            
        asyncio.create_task(self.coroutine_start())    
        
        threading.Thread(target=self.video_stream , daemon=True).start()
        # asyncio.create_task(self.sender())
        # asyncio.create_task(self.receiver())
        # asyncio.create_task(self.update_state())
        # asyncio.create_task(self.video_stream())

    #             # self.initialize(),
    #             self.sender(),
    #             self.receiver(),
    #             self.update_state(),
    #             self.video_stream()         

    def __del__(self):
        super().__del__()
        print("I'm Dead.")
        

    # def _cleanup(self):
    #     self.is_running = False
    #     print(f"{self.sn}: Deleted")
        
    # def __del__(self):
    #     print("child")
    #     super().__del__()
                         
    async def initialize(self):
        self.ep_robot = robot.Robot()
        try:
            self.ep_robot.initialize(conn_type='sta', sn= self.sn)
            self.ep_chassis = self.ep_robot.chassis
            self.ep_camera = self.ep_robot.camera
            
            # Add
            self.ep_robot.armor.set_hit_sensitivity(comp="all", sensitivity=100)
            self.ep_robot.armor.sub_hit_event(self.hit_callback)
            self.ep_robot.sensor.sub_distance(freq=20, callback=self.tof_callback)            
            
            self.start_stream()        
        
            await self.rep_queue.put(0)       
            
        except Exception as e:
            await self.rep_queue.put(1)

    async def command(self, cmd):
        await self.cmd_queue.put(cmd)
    
    async def sender(self):
        while self.is_running:
            cmd = await self.cmd_queue.get()
            try:
                if cmd == CMD_CHASSIS_X_UP:
                    self.ep_chassis.move(x=0.3, y=0, z=0, xy_speed=3).wait_for_completed()
                    # self.ep_chassis.drive_speed(x=1, y=0, z=0, timeout=2)
                    
                    # self.ep_chassis.move(x=0.3, y=0, z=0, xy_speed=1)
                elif cmd == CMD_CHASSIS_X_DOWN:
                    # self.ep_chassis.move(x=-0.1, y=0, z=0, xy_speed=1).wait_for_completed()
                    self.ep_chassis.move(x=-0.3, y=0, z=0, xy_speed=1).wait_for_completed()
                    
                elif cmd == CMD_CHASSIS_Y_UP:
                    self.ep_chassis.move(x=0, y=0.1, z=0, xy_speed=1).wait_for_completed()
                    # self.ep_chassis.move(x=0, y=0.3, z=0, xy_speed=1)
                    
                elif cmd == CMD_CHASSIS_Y_DOWN:
                    self.ep_chassis.move(x=0, y=-0.1, z=0, xy_speed=1).wait_for_completed()
                    # self.ep_chassis.move(x=0, y=-0.3, z=0, xy_speed=1)
                    
                await self.rep_queue.put(0)
                
            except:
                await self.rep_queue.put(1)
    
    
    async def receiver(self):
        pass
    
    async def update_state(self):
        pass
    
    def video_stream(self):
        while self.is_running:
            if self.is_stream:
                try:
                    self.image = self.ep_camera.read_cv2_image(strategy="newest")
                    # print(1)
                    # if self.video_queue.full():
                    #     self.video_queue.get()
                    #     self.video_queue.put(img)         
                    
                    # else:
                    #     self.video_queue.put(img)            
                        
                except:
                    continue    
        
    def start_stream(self):
        if not self.is_stream:
            self.ep_camera.start_video_stream(display=False, resolution=camera.STREAM_360P)
            self.is_stream = True
    
    def stop_stream(self):
        if self.is_stream:
            self.ep_camera.stop_video_stream()
            self.is_stream = False
    
    def hit_callback(self, sub_info):
        print(f"sub info : {sub_info}")
        # pass
    
    def tof_callback(self, tof_info):
        # print(tof_info)
        # print(type(tof_info))
        self.distance = tof_info[0]
    
        
        
    async def coroutine_start(self):   
        self.async_tasks = [
                # self.initialize(),
                self.sender(),
                self.receiver(),
                self.update_state(),
                # self.video_stream()            
        ]

        await asyncio.gather(*self.async_tasks)    
    