from os.path import dirname, abspath
from pathlib import Path
from fastapi import APIRouter, Depends, Request, responses, status
from ..models.simulator import Simulator
import json
import queue
import threading
from ..models.base_model import StateRequest


router = APIRouter()

sim_send_queue = queue.Queue()
sim_recv_queue = queue.Queue()
sim_event = threading.Event()


sim = Simulator(sim_send_queue, sim_recv_queue, sim_event)

sim.engine_thread_start()

@router.post("/state/")
def control(request: Request, state_request: StateRequest):
    sim_send_queue.put(state_request)
    # sim_event.set()
    
    sim_event.wait()
    sim_event.clear()
    
        
    state_response = sim_recv_queue.get()
    # print("From Simulator : ", text)
    # print(state_response)
    
    return state_response
    
    
    
    
    
# @router.post("/control/")
# def control(request: Request, drone_control: DroneControl):
#     """드론 원격 제어를 위한 코드

#     Args:
#         drone_id (str): 드론 ID
#         cmd (str): 제어문

#     Returns:
#         _type_: _description_
#     """
        
#     if drone_control.drone_id in drones:
#         drones[drone_control.drone_id].command(drone_control.cmd)
#         return {"status": "ok", "msg" : f"{drone_control.cmd} Commanded."}
        
#     else:
        
#         try:
#             drone_ip = ip_table_dict[drone_control.drone_id]["ip_address"]
            
#         except Exception as e:
#             return {"status": "err", "msg" : f"No Drone IP Found."}
            
#         drones[drone_control.drone_id] = Tello(drone_ip, 8889, 8890, 11111)
#         drones[drone_control.drone_id].command(drone_control.cmd)
        
#         return {"status" : "ok", "msg": f"{drone_control.drone_id} : New Model Created. {drone_control.cmd} Commanded."}    
    
    
    
# @router.delete("/drone/{drone_id}")
# def delete_drone(request: Request, drone_id: str):
    
#     if drone_id in drones:
        
#         del drones[drone_id]
#         return {"status" : "ok", "msg": f"{drone_id} Deleted."} 
    
#     else:
#         return {"status" : "err", "msg": f"No {drone_id} Found."} 
    
    
# @router.post("/create")
# def create(request: Request):
#     pass


# @router.get("/register/")
# def render_register(request: Request):
#     return templates.TemplateResponse("auth/register.html", {"request": request})

# @router.post("/register/")
# async def handle_register(request: Request):
#     form = UserRegisterForm(request)
#     await form.load_data()
#     if await form.is_valid():
#         if await db_manager.create_user(form):
#             return responses.RedirectResponse(
#                 "/?msg=Successfully-Registered", status_code=status.HTTP_302_FOUND
#             )  # default is post request, to use get request added status code 302
#         else:
#             form.__dict__.get("errors").append("Duplicate username or email")
#             return templates.TemplateResponse("auth/register.html", form.__dict__)
            
#     return templates.TemplateResponse("auth/register.html", form.__dict__)