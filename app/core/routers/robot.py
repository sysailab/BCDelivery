from os.path import dirname, abspath
from pathlib import Path
from fastapi import APIRouter, Depends, Request, responses, status, Response
from typing import List
from ..models.robo_ep import RoboEP
import json
import queue
import threading
from ..models.base_model import Control
import asyncio
from robomaster import conn
import time
from datetime import datetime
import io
import base64
from starlette.responses import StreamingResponse
import cv2

router = APIRouter()

robots = {}

robot_ip_table = open("app/core/routers/robot_ip_table.json", 'r')
robot_ip_table = json.load(robot_ip_table)

ip_dict = None

def robot_scan():
    global ip_dict
    while True:
        ip_dict = conn.scan_robot_ip_list(timeout=5)
        time.sleep(10)

threading.Thread(target=robot_scan, daemon=True).start()

# async def robot_scan():
#     global ip_dict
#     while True:
#         ip_dict = conn.scan_robot_ip_list(timeout=0.5)
#         await asyncio.sleep(10)
        
# asyncio.create_task(robot_scan())

def ok_command(obj_type, ) -> str:
    return 
    







@router.post("/control/")
async def control(request: Request, robot_control: Control):
    
    if robot_control.id in robots:
        await robots[robot_control.id].command(robot_control.cmd)
        
        robot_rep = await robots[robot_control.id].rep_queue.get()
    
        if robot_rep == 0:
            return {"status": "ok", "msg" : f"{robot_control.cmd} Commanded."}
        
        elif robot_rep == 1:
            return {"status": "err", "msg" : f"{robot_control.id} Robot did not responsed."}
            
        else:
            return {"status ": "except", "msg" : f"{robot_rep}"}
    
    else:
        if not robot_initialize(robot_control.id):
            return {"status": "err", "msg" : f"No Robot ID Found."}
        
        # 로봇 수신 가능 확인
        await robots[robot_control.id].initialize()
                
        if await robots[robot_control.id].rep_queue.get() == 1:
            del robots[robot_control.id]
            return {"status": "err", "msg" : f"Robot did not responsed."}   
        
        await robots[robot_control.id].command(robot_control.cmd)
        
        robot_rep = await robots[robot_control.id].rep_queue.get()
    
        if robot_rep == 0:
            return {"status": "ok", "msg" : f"{robot_control.cmd} Commanded."}
        
        elif robot_rep == 1:
            return {"status": "err", "msg" : f"{robot_control.id} Robot did not responsed."}
            
        else:
            return {"status ": "except", "msg" : f"{robot_rep}"}
              
@router.get("/control")
async def control_sn(request: Request, robot_sn:str, cmd:str):

    if robot_sn in robots:
        await robots[robot_sn].command(cmd)
        
        robot_rep = await robots[robot_sn].rep_queue.get()
    
        if robot_rep == 0:
            return {"status": "ok", "msg" : f"{robot_sn} Commanded."}
        
        elif robot_rep == 1:
            return {"status": "err", "msg" : f"{robot_sn} Robot did not responsed."}
            
        else:
            return {"status ": "except", "msg" : f"{robot_rep}"}
    
    else:
        print("Try To Generated Robot...")
        if not robot_initialize(robot_sn):
            return {"status": "err", "msg" : f"No Robot ID Found."}
        
        # 로봇 수신 가능 확인
        await robots[robot_sn].initialize()
                
        if await robots[robot_sn].rep_queue.get() == 1:
            del robots[robot_sn]
            return {"status": "err", "msg" : f"Robot did not responsed."}   
        
        await robots[robot_sn].command(robot_sn)
        
        robot_rep = await robots[robot_sn].rep_queue.get()
    
        if robot_rep == 0:
            return {"status": "ok", "msg" : f"{robot_sn} Commanded."}
        
        elif robot_rep == 1:
            return {"status": "err", "msg" : f"{robot_sn} Robot did not responsed."}
            
        else:
            return {"status ": "except", "msg" : f"{robot_rep}"}    
      
@router.get("/scan/")
async def control(request: Request):
    # ip_list = conn.scan_robot_ip_list(timeout=1)
    return ip_dict

@router.get("/info")
async def control(request: Request, robot_sn: str):
    if robot_sn in robots:
        if not robots[robot_sn].is_stream:
            return {"status ": "except", "msg" : "Robot is not streaming."}   
        
        else:
            try:
                # image_data = robots[robot_sn].video_queue.get()
                image_data = base64.b64encode(robots[robot_sn].video_queue.get())
                # decode_data = base64.b64encode(image_data)
                # print(image_data)
            except:
                return {"status ": "except", "msg" : "No Img Found."}   
            
            data = {
                "id": robot_sn,
                "time": datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f'),
                "imageData": image_data,
                "distance": robots[robot_sn].distance
            }
            # robots[robot_sn]
            
            return data
    
    else:
        return {"status ": "except", "msg" : "No Robot Found. Check SN."}    
     
@router.get("/video")
def stream_video(request: Request, robot_sn: str):
    
    if robot_sn in robots:
        try:
            frame = robots[robot_sn].video_queue.get_nowait()
            ret, buffer = cv2.imencode('.jpg', frame)
            
            if ret:
                video_frame = buffer.tobytes()  
                return StreamingResponse(io.BytesIO(video_frame), media_type="image/jpeg")
            else:
                return {"status": "err", "msg" : f"Could not convert img."}      
            
        
        except queue.Empty:
            return Response(content="No frame available. Maybe Drone is turned off.", status_code=500)
        
    else:     
        if not robot_initialize(robot_sn):
            return {"status": "err", "msg" : f"No Drone ID Found."}        
    
        try:                    
            # frame = drones[drone_id].drone_video_queue.get_nowait()
            frame = robots[robot_sn].video_queue.get_nowait()
            ret, buffer = cv2.imencode('.jpg', frame)
            
            if ret:
                video_frame = buffer.tobytes()  
                return StreamingResponse(io.BytesIO(video_frame), media_type="image/jpeg")
            else:
                return {"status": "err", "msg" : f"Could not convert img."}  
            
        except:
            # print("err")
            return Response(content= "No frame available. Maybe Drone is turned off", status_code=500)
            
        # return Response(content= "임시 에러", status_code=500)
     
     
@router.delete("/{robot_sn}")
async def control(request: Request, robot_sn: str):
    
    if robot_sn in robots:
        del robots[robot_sn]
        
        return {"Robot Deleted"}
    
    else:
        
        return {"No Robot Found"}

def robot_initialize(_robot_sn) -> RoboEP:
    # try:
    #     robot_ip = robot_ip_table[_robot_sn]["ip_address"]
                
    # except Exception as e:
    #     return False
    
    # robot_sn = robot_ip_table[_robot_sn]["sn"]
    
    robots[_robot_sn] = RoboEP(_robot_sn)
    
    return True