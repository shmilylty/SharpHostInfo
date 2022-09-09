# SharpHostInfo

## ✨ 项目简介

🦄 **SharpHostInfo是一款快速探测内网主机信息工具（深信服深蓝实验室天威战队强力驱动）**

![image](https://user-images.githubusercontent.com/24275308/189285126-e2fefeea-ffb7-4917-9adc-9eecc9d27820.png)

在内网渗透中，尤其是域环境里，有时我们想知道一些内网主机信息开展进一步渗透，比如内网有哪些主机，哪些主机是域控？

SharpHostInfo正是一款应用而生的小工具，体积较小，速度极快，支持NetBIOS、SMB和WMI服务快速探测，希望能帮到你！

## 🚀 快速使用

1. 查看帮助信息

    ```bash
    SharpHostInfo.exe -h
    ```

2. 使用默认参数进行探测

   指定探测网段为192.168.1.1/24，默认使用NetBIOS和SMB服务进行探测进行自动探测，默认线程100个，默认探测超时500毫秒。
   
    ```bash
    SharpHostInfo.exe -i 192.168.1.1/24
    ```

3. 使用NetBIOS和WMI服务进行探测

    指定探测网段为192.168.1.1/16，指定探测线程为200个，指定探测超时1000毫秒。

    ```bash
    SharpHostInfo.exe -s nbns,wmi -i 192.168.1.1/16 -t 200 -m 1000
    ```
    你也可以使用长参数的形式：
    ```bash
    SharpHostInfo.exe --service=nbns,wmi --target=192.168.1.1/16 --thread=200 --timeout=1000
    ```

4. 其他使用说明

    SharpHostInfo自带了一份全球所有组织申请MAC地址对应文件`manuf.json`，可以根据MAC地址判断对应设备所属公司。
    这样可以粗略判断设备是否是安全设备或重要设备，只需把此文件放到SharpHostInfo.exe同目录下即可自行调用。
    当然SharpHostInfo不需要此文件也可以正常运行，欢迎提交常见重要设备MAC指纹，你可以在[这里](https://github.com/shmilylty/SharpHostInfo/blob/master/SharpHostInfo/Helpers/Options.cs#L26)看到硬编码的指纹。


## 😸 帮助信息

```text
 __ _                                      _    _____        __
/ _\ |__   __ _ _ __ _ __   /\  /\___  ___| |_  \_   \_ __  / _| ___
\ \| '_ \ / _` | '__| '_ \ / /_/ / _ \/ __| __|  / /\/ '_ \| |_ / _ \
_\ \ | | | (_| | |  | |_) / __  / (_) \__ \ |_/\/ /_ | | | |  _| (_) |
\__/_| |_|\__,_|_|  | .__/\/ /_/ \___/|___/\__\____/ |_| |_|_|  \___/
                    |_|           Version: 0.0.1


USAGE:
   SharpHostInfo.exe [arguments...]

ARGUMENTS:
   --target,  -i   目标IP(必须参数)
   --thread,  -t   探测线程(默认: 100)
   --timeout, -m   探测超时(默认: 500)
   --service, -s   指定使用探测服务(默认: nbns,smb)

EXAMPLES:
   SharpHostInfo.exe --target=192.168.1.1
   SharpHostInfo.exe --target=ip.txt --threads=40 -s nbns,wmi
   SharpHostInfo.exe --target=192.168.1.1-192.168.255.255 --timeout=1000
   SharpHostInfo.exe -i 192.168.1.1,192.168.1.2 -s nbns
   SharpHostInfo.exe -i 192.168.1.1/20 -t 200 -m 1000 -s wmi
```

## 🔫 功能特性

* c#编写，可内存执行，免杀友好。
* 小巧轻便，速度极快。
* 支持NetBIOS(默认137端口)、SMB(默认135端口)和WMI(默认135端口)服务快速探测
* 支持自动识别域控主机
* 支持自动识别MAC地址对应设备

## 🏂 计划功能

* [ ] 支持自动保存探测结果

欢迎反馈贴近实战的建议！

## 😽 鸣谢

感谢网上开源的相关项目！

## 📜 免责声明

本工具仅能在取得足够合法授权的企业安全建设中使用，在使用本工具过程中，您应确保自己所有行为符合当地的法律法规。
如您在使用本工具的过程中存在任何非法行为，您将自行承担所有后果，本工具所有开发者和所有贡献者不承担任何法律及连带责任。
除非您已充分阅读、完全理解并接受本协议所有条款，否则，请您不要安装并使用本工具。
您的使用行为或者您以其他任何明示或者默示方式表示接受本协议的，即视为您已阅读并同意本协议的约束。

## 💖Star趋势

[![Stargazers over time](https://starchart.cc/shmilylty/SharpHostInfo.svg)](https://starchart.cc/shmilylty/SharpHostInfo)
   
