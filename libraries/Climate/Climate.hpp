//
//  Climate.hpp
//  
//
//  Created by Tommy Kardach on 4/1/18.
//
//

#ifndef Climate_hpp
#define Climate_hpp

#include <stdio.h>
#include "DHT.h"

namespace Climate  {

    class IClimate {
    public:
        /**
            Temperature in degrees Farenheight
            @return temperature as a float
         */
        virtual float getTemperatureF();
        
        /**
            Temperature in degrees Celcius
            @return temperature as a float
         */
        virtual float getTemperatureC();
        
        /**
            Percent humidity
            @return humidity as a float
         */
        virtual float getHumidity();
    };
  
    class SensorClimate : public IClimate {
    private:
        DHT *dht;
    public:
        /**
            Creates a SensorClimate with the specified DHT sensor information.
            @param pin The pin number used by the DHT sensor
            @param type The type of DHT sensor
         */
        SensorClimate(uint8_t pin, uint8_t type);
        ~SensorClimate();
        
        virtual float getTemperatureF();
        virtual float getTemperatureC();
        virtual float getHumidity();
    };
}

#endif /* Climate_hpp */
