from os.path import dirname, abspath
from pathlib import Path
from fastapi import APIRouter, Depends, Request, responses, status
from typing import List
from ..models.robo_ep import RoboEP
import json
import queue
import threading
from ..models.base_model import Control
import asyncio

router = APIRouter()

robots = {}

robot_ip_table = open("app/core/routers/robot_ip_table.json", 'r')
robot_ip_table = json.load(robot_ip_table)

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
                
    
    
def robot_initialize(_robot_id) -> RoboEP:
    try:
        robot_ip = robot_ip_table[_robot_id]["ip_address"]
                
    except Exception as e:
        return False
    
    robot_sn = robot_ip_table[_robot_id]["sn"]
    
    robots[_robot_id] = RoboEP(robot_sn)
    
    return True
