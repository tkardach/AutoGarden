import json

def test_string_load():
   dict = {}
   for x in range(0,1000):
      dict["att" + str(x)] = "My Value " + str(x)
   return json.dumps(dict)
