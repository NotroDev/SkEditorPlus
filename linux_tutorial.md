## Linux Workaround
This tutorial is going to explain to you how to run SkEditorPlus on Linux using Wine and Bottles

NOTE: This workaround is not official, its not supported by SkEditorPlus 

---

1. Install [Bottles](https://usebottles.com/)  
*Note: make sure flatpak is installed on your system, [install guide](https://flatpak.org/setup/)*   
```flatpak install flathub com.use bottles.bottles```

2. Open bottles, and create a new bottle with any name, and the "Application" environment

<img src="https://user-images.githubusercontent.com/79372025/230179920-eaf23f9c-904f-43ad-8c45-fd0569fb6f8c.png" width=40% height=40%>
3. In the bottles Settings tab set DXVK & VKD3D to Disabled

<img src="https://user-images.githubusercontent.com/79372025/230181066-5e0e367b-334d-48b2-9d7e-c7fbdb7290ad.png" width=40% height=40%>
4. In the bottles Dependencies tab, install "dotnetcoredesktop6"

<img src="https://user-images.githubusercontent.com/79372025/230181452-2db81d0d-edcb-49db-9487-d735870cb19f.png" width=40% height=40%>
5. You can start `SkEditorPlus.exe` using the "Run Executable" Button  

   
   
![image](https://user-images.githubusercontent.com/79372025/230182982-9264a69b-658c-4cad-b29e-93f797c28bcf.png)
