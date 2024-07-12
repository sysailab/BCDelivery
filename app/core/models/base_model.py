from pydantic import BaseModel
from typing import Optional

class DroneControl(BaseModel):
    """ Drone Control : For Post

    Required Value: 
        drone_id: str
        cmd: str
    
    Optional Value:
        description: str
    """
    
    drone_id: str
    cmd: str
    description: Optional[str]
    
class StateRequest(BaseModel):
    """ Request State From Unity

    Required Value: 
        drone_id: str
        home: int
        store: int
        state: str 
    """
    
    
    id: str
    home: int
    store: int
    state: str
