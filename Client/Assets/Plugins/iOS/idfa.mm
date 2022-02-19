// #import <AdSupport/ASIdentifierManager.h>

// extern "C" {

// static const char* MakeStringCopy(const char* string)
// {
//     if (string == NULL)
//         return NULL;
//     char* res = (char*)malloc(strlen(string) + 1);
//     strcpy(res, string);
//     return res;
// }

// const char* getIdfa()
// {
//     if([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled])
//     {
//         NSString * idfa = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
//         return MakeStringCopy([idfa UTF8String]);
//     }
//     return nil;
// }

// }