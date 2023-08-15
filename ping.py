import sys
import time
from requests import get, head
from colorama import Fore, Style

HALT = 30
TARGET_URL = "https://aqarathammad.com/" #"http://localhost:5000/"

def scope(seconds):
    try:
        while True:
            timeX = time.localtime()
            current_time = time.strftime("%H:%M:%S", timeX)
            scopeX = head(TARGET_URL)
            
            if scopeX.status_code == 405:
                scopeX = get(TARGET_URL)
            
            if scopeX.status_code == 200 or scopeX.status_code == 405:
                print(Fore.GREEN + f"[+] -INFO:: Target website is =====[ UP ]===== and loaded successfully, Response code is# {scopeX.status_code} current time now is# {current_time}.")
                print(Style.RESET_ALL + f"[+] -INFO:: I will sleep for {HALT} seconds now, Just press CTEL + C if tyou want to stop this script.")
            else:
                print(Fore.RED + f"[-] -INFO:: Target website is =====[ DOWN ]===== and can not be loaded, Response code is# {scopeX.status_code} and current time now is# {current_time}.")
                print(Style.RESET_ALL + f"[+] -INFO:: I will sleep for {HALT} seconds now then try again, You can press CTEL + C if tyou want to stop this script.")
                
            time.sleep(seconds)
    except KeyboardInterrupt:
        sys.exit()
    except Exception as err:
        print('__main.function_Error at line', sys.exc_info()[-1].tb_lineno, err)
        scope(HALT)
        
if __name__ == "__main__":
    try:
        scope(HALT)
    except KeyboardInterrupt:
        sys.exit()
    except Exception as err:
        print('__main.function_Error at line', sys.exc_info()[-1].tb_lineno, err)
        scope(HALT)
