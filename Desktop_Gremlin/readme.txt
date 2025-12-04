Author: Kritzkingvoid
Repo: https://github.com/KurtVelasco/Desktop_Gremlin
Donate: https://ko-fi.com/kritzkingvoid

Built using WPF C#
Compatible with Windows 10/11  7* With some Aero Settings

 @"KEYBOARD CONTROLS: Can be Turned off in Config.txt

                MOVEMENT (Disabled in Combat Mode):
                    WASD / Arrow Keys - Move character
                    E - Toggle cursor following
                    R - Random movement
                    ESC - Stop all movement

                ACTIONS:
                    SPACE - Click animation
                    T - Toggle sleep/wake
                    C - Toggle Gravity                  
                    Q - Spawn Food
                    1 - Emote 1 (combat only)
                    2 - Emote 3 (combat only)
                    3 - Emote 2 (combat only)
                    4 - Emote 4 (combat only)
                HELP:
                    F1 - Show this help
                    X - Close Program
                    0/Zero - Disable Hitbox //Non-Mouse Interactable";    


Basic Troubleshooting

Q: Is this program compatible with later version x, etc?
A: No, this program is different from the others in terms of coding structure. Maybe in the future once I manage to make
   a perfect foundation so I can import all the other umas, but right now, each Uma is unique in terms of code.

Q: The Gremlin is big
A: Play around with this settings in config.txt ENABLE_MIN_RESIZE, FORCE_CENTER, ENABLE_MANUAL_RESIZE,
If it's still big, then I have no idea its prob your DPI/Screen Resolutions Settings
 //Also there's a problem with some Machines no in "ENGLISH" due to converting '.' to ',' in the config.txt

Q: Browser falsely detects download 
A: Try using a different browser or temporarily disable strict security settings. 

Q: Gremlin not following my mouse
A: Try lowering the FOLLOW_RADIUS in config.txt 

Q: Gremlin not animating while dragging 
A: Performance options in windows. Check "Animate controls and elements inside windows" 

Q: Transparency not working : 
A: Settings → Personalization → Colors → Transparency Effects → On **OR** Settings → Accessibility → Contrast Themes → None |

Q: Transparency not working : 
A: Set FAKE_TRANSPARENCY to TRUE in config.txt

Q: Transparency still not working : 
A: If your using Win7 then change the Aero settings to Appearance | or your driver acceleration is off/outdated ppearance | or your driver acceleration is off/outdated 