//
//  Climate.cpp
//
//
//  Created by Tommy Kardach on 4/1/18.
//
//

#include "Climate.hpp"

namespace Climate {
    
    SensorClimate::SensorClimate(uint8_t pin, uint8_t type) {
        dht = new DHT(pin, type);
        dht->begin();
    }
    
    float SensorClimate::getTemperatureC() {
        float c = dht->readTemperature();
        
        if (isnan(c)) return NAN;
        return c;
    }
    
    float SensorClimate::getTemperatureF() {
        float f = dht->readTemperature(true);
        
        if (isnan(f)) return NAN;
        return f;
    }
    
    float SensorClimate::getHumidity() {
        float h = dht->readHumidity();
        
        if (isnan(h)) return NAN;
        return h;
    }
}
