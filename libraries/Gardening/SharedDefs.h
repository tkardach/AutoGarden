//
//  SharedDefs.h
//  
//
//  Created by Tommy Kardach on 4/3/18.
//
//

#ifndef SharedDefs_h
#define SharedDefs_h

namespace Shared { namespace Arduino {
    namespace Hardware {
        typedef struct {
            char[] address;
            char[] type;
            uint8_t pin;
        } HardwareInfo;
    }
}}


#endif /* SharedDefs_h */
