from pyevsim import BehaviorModelExecutor, Infinite, SysMessage
from .device_info import DRONE, CAR, TRANSITIONS
import random

class ScenarioGenerator(BehaviorModelExecutor):
    def __init__(self, instance_time, destruct_time, name, engine_name):
        BehaviorModelExecutor.__init__(self, instance_time, destruct_time, name, engine_name)
        
        self.init_state("IDLE")
        self.insert_state("IDLE", Infinite)
        self.insert_state("PROCESSING", 1)

        self.insert_input_port("generate")
        self.insert_output_port("fin")

    def ext_trans(self, port, msg):
        if port == "generate":
            self.data_msg = msg.retrieve()[0]
            self._cur_state = "PROCESSING"

    def output(self):
        if self._cur_state == "PROCESSING":
            random_data = self.data_msg

            for rd in random_data:
                if rd['state'] == "None":
                    rd['state'] = "STAY"
                else:
                    self.transition_state(rd, random_data)

            msg = SysMessage(self.get_name(), "fin")
            msg.insert(random_data)
            # print(" + ", random_data)
            return msg

    def int_trans(self):
        if self._cur_state == "PROCESSING":
            self._cur_state = "IDLE"

    def transition_state(self, device, random_data):
        current_state = device['state']
        if current_state not in TRANSITIONS:
            return

        possible_states = list(TRANSITIONS[current_state].keys())
        weights = list(TRANSITIONS[current_state].values())
        selected_state = random.choices(possible_states, weights)[0]

        if current_state == "ACCIDENT":
            return  # ACCIDENT 상태에서 다른 상태로 전환 불가

        if current_state == "DELIVERY" and selected_state == "DELIVERY":
            pass
        else:
            device['state'] = selected_state

            if selected_state == "DELIVERY":
                numbers = random.sample(range(1, 67), 2)
                device['home'] = numbers[0]
                device['store'] = numbers[1]
            elif selected_state == "ACCIDENT":
                self.handle_accident_state(device, random_data)
            elif selected_state == "CANCEL":
                device['state'] = 'STAY'
                device['home'] = 0
                device['store'] = 0

    def handle_accident_state(self, accident_device, random_data):
        stay_devices = [d for d in random_data if d['state'] == 'STAY']
        if stay_devices:
            stay_device = random.choice(stay_devices)
            stay_device['home'] = accident_device['home']
            stay_device['store'] = accident_device['store']
            stay_device['state'] = 'DELIVERY'