//
//  Climate.cpp
//
//
//  Created by Tommy Kardach on 4/1/18.
//
//

#include "Climate.h"

DHTClimate::DHTClimate(uint8_t pin, uint8_t type) {
	dht = new DHT(pin, type);
	dht->begin();
}

DHTClimate::~DHTClimate() {
	delete dht;
}

float DHTClimate::getTemperatureC() {
	float c = dht->readTemperature();
	
	if (isnan(c)) return NAN;
	return c;
}

float DHTClimate::getTemperatureF() {
	float f = dht->readTemperature(true);
	
	if (isnan(f)) return NAN;
	return f;
}

float DHTClimate::getHumidity() {
	float h = dht->readHumidity();
	
	if (isnan(h)) return NAN;
	return h;
}

