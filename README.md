# AutoGarden
The purpose of this project is to create an automated gardening system. Plans for this project include:

 -Watering unique plants with different watering plans, following a user set schedule
 -Using different watering methods, such as potted plants versus hydroponic methods
 -Maintaining greenhouse climate

This project will be split into three different parts: 

 -Creating bluetooth peripheral devices
 -Creating a bluetooth host to issue commands to peripherals
 -Creating a mobile application to monitor and set data within the bluetooth host.

The bluetooth peripherals will use Bluetooth Low Energy and will be responsible for receiving commands from the host and implementing those requests.

The bluetooth host will be the brains of the operation. It will have a locally stored database to keep track of important information, and it will act as a server for the mobile application.

The mobile application will allow the user to add plants to be monitored and add a greenhouse to be monitored. It will be responsible for establishing a connection between the bluetooth host and peripherals and creating a an interaction plan between the two devices depending on the peripheral's purpose.

The pre-coding phase system design looks as follows:

![alt-text](https://github.com/tkardach/AutoGarden/blob/master/Design/UML_v1.png)
