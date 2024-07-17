import asyncio
import robomaster
from robomaster import robot, camera
import time
from .base_model import BaseRobot
import threading

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
        
        self.is_stream = False
            
        
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
        
                        
    async def initialize(self):
        self.ep_robot = robot.Robot()
        try:
            self.ep_robot.initialize(conn_type='sta', sn= self.sn)
            self.ep_chassis = self.ep_robot.chassis
            self.ep_camera = self.ep_robot.camera     
            await self.rep_queue.put(0)       
            
        except Exception as e:
            await self.rep_queue.put(1)

    async def command(self, cmd):
        await self.cmd_queue.put(cmd)
    
    async def sender(self):
        while True:
            cmd = await self.cmd_queue.get()
            try:
                if cmd == CMD_CHASSIS_X_UP:
                    # self.ep_chassis.move(x=0.1, y=0, z=0, xy_speed=1).wait_for_completed()
                    self.ep_chassis.drive_speed(x=1, y=0, z=0, timeout=2)
                    
                    # self.ep_chassis.move(x=0.3, y=0, z=0, xy_speed=1)
                    
                elif cmd == CMD_CHASSIS_X_DOWN:
                    self.ep_chassis.move(x=-0.1, y=0, z=0, xy_speed=1).wait_for_completed()
                    # self.ep_chassis.move(x=-0.3, y=0, z=0, xy_speed=1)
                    self.ep_chassis.drive_speed(x=0, y=0, z=0, timeout=2)
                    
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
        while True:
            if self.is_stream:
                try:
                    img = self.ep_camera.read_cv2_image(strategy="newest")
                    # print(1)
                    if self.video_queue.full():
                        self.video_queue.get()
                        self.video_queue.put(img)         
                    
                    else:
                        self.video_queue.put(img)            
                        
                except:
                    self.video_queue.put(1)
            # else:
            #     print("No Stream")
        
        
    def start_stream(self):
        if not self.is_stream:
            self.ep_camera.start_video_stream(display=False, resolution=camera.STREAM_360P)
            self.is_stream = True
    
    def stop_stream(self):
        if self.is_stream:
            self.ep_camera.stop_video_stream()
            self.is_stream = False
        
    async def coroutine_start(self):   
        self.async_tasks = [
                # self.initialize(),
                self.sender(),
                self.receiver(),
                self.update_state(),
                # self.video_stream()            
        ]

        await asyncio.gather(*self.async_tasks)
        
        # asyncio.gather(*self.async_tasks)
        
#     async def robot_close(self):
#         self.ep_robot.close()
        
        
#     def robot_command(self):
#         pass
    
#     async def robot_camera(self):
#         pass
    
#     async def robo_init(self):
#         await asyncio.gather(
#                         self.perform_task(1),
#                         self.perform_task(2),
#                         self.perform_task(3)
#                     )
        
#     async def perform_task(self, task_id):
#         print(f"{task_id} will perform")
#         total = 0
#         for i in range(101):
#             total += i
#             print(f"{self.obj_num} / {task_id} : total = {total}")
#             await asyncio.sleep(10 / 100)        
    
# async def roop_exe():
#     i = 1
#     while True:
#         print("while")
#         # await asyncio.gather(RoboEP(i).robo_init())
#         asyncio.create_task(RoboEP(i).robo_init())
#         await asyncio.sleep(1)
#         i += 1    
    

    
# if __name__ == "__main__":
#     asyncio.run(RoboEP("3JKCK980030EKR").robot_initialize())
    
    # asyncio.run(roop_exe())
    # i = 1
    # while True:
    #     print("while")
    #     asyncio.gather(RoboEP(i).robo_init())
    #     time.sleep(1)
    #     i += 1
        # robo_ep1 = RoboEP
        # robo_ep2 = RoboEP
        
        # asyncio.run(init())
    
    
    