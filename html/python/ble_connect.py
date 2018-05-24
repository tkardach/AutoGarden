from bluepy import btle

dev = btle.Peripheral("30:ae:a4:7b:01:6a")

dev.getServices()[2].getCharacteristics()[1].write("1")

dev.getServices()[2].getCharacteristics()[2].read()

