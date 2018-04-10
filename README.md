# AutoGarden
The purpose of this project is to become a better programmer and to expand my skill set and understanding of full stack programming.

The objective of this project is to create an automated gardening system. Plans for this project include: <br/>
<ul>
 <li>Watering unique plants with different watering plans, following a user set schedule</li>
 <li>Using different watering methods, such as potted plants versus hydroponic methods</li>
 <li>Maintaining greenhouse climate</li>
</ul>

This project will be split into three different parts: <br/>
<ul>
 <li>Creating bluetooth peripheral devices</li>
 <li>Creating a bluetooth host to issue commands to peripherals</li>
 <li>Creating a mobile application to monitor and set data within the bluetooth host</li>
</ul>

The bluetooth peripherals will use Bluetooth Low Energy and will be responsible for receiving commands from the host and implementing those requests.

The bluetooth host will be the brains of the operation. It will have a locally stored database to keep track of important information, and it will act as a server for the mobile application.

The mobile application will allow the user to add plants to be monitored and add a greenhouse to be monitored. It will be responsible for establishing a connection between the bluetooth host and peripherals and creating a an interaction plan between the two devices depending on the peripheral's purpose.

The pre-coding phase system design looks as follows:

![alt-text](https://github.com/tkardach/AutoGarden/blob/master/Design/UML_v1.png)



# Referenced Libraries
<ul>
 <li>https://github.com/adafruit/Adafruit_Sensor</li>
 <li>https://github.com/adafruit/DHT-sensor-library</li>
</ul>
