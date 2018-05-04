import pickle
import os

def pickle_get(file):
   try:
      with open(file, 'rb') as f:
         list = pickle.load(f)
         return list
   except Exception as e:
      print "Pickle get : " + str(e)
      return None

def pickle_remove(file, obj):
   try:
      list = []
      if not os.stat(file).st_size == 0:
         with open(file, 'rb') as f:
            list = pickle.load(f)
      with open(file, 'wb') as f:
         if obj not in list:
            return False
         else:
            list.remove(obj)
         pickle.dump(list, f)
      return True
   except Exception as e:
      print "Pickle Remove : " + str(e)
      return False

def pickle_add(file, obj):
   try:
      list = []
      if not os.stat(file).st_size == 0:
         with open(file, 'rb') as f:
            list = pickle.load(f)
      with open(file, 'wb') as f:
         if obj not in list:
            list.append(obj)
         pickle.dump(list, f)
      return True
   except Exception as e:
      print "Pickle Add : " + str(e)
      return False

