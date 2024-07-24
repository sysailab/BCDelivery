class ResponseFormat:
    @staticmethod
    def ok_command(id, cmd):
        return {"status": "ok", "msg" : f"{id} : {cmd} Commanded."}

    @staticmethod
    def err_command(id):
        return {"status": "err", "msg" : f"{id} : Robot did not responsed."}

    @staticmethod
    def err_found(id):
        return {"status": "err", "msg" : f"{id} : No Robot ID Found."}

    @staticmethod
    def err_except():
        return {"status ": "except", "msg" : "Exception found in server."}
    
    @staticmethod
    def err_stream(id):
        return {"status ": "err", "msg" : f"{id} : Robot is not streaming."} 
    
    @staticmethod
    def err_no_data(id):
        return {"status ": "err", "msg" : f"{id} : Robot data not found."}     
    
    @staticmethod
    def err_convert():
        return {"status": "err", "msg" : f"Could not convert img."}

    @staticmethod
    def ok_delete(id):
        return {"status": "ok", "msg" : f"{id} : Object Deleted."}

    @staticmethod
    def ok_scan(_ip_dict):
        return _ip_dict
        
    @staticmethod
    def ok_info(id, time, imageData, distance):
        return {
            "id": id,
            "time": time,
            "imageData": imageData,
            "distance": distance           
        }
        
        
        
    # @staticmethod
    # def err_obj_response():
    #     pass

