
#include <stdint.h>

typedef enum { false, true } bool;
// typedef unsigned char byte;
// typedef long long int64;

#pragma pack(push,1)
typedef struct {
	char *Ip;
	char *Port;
	bool IsHttps;
	char *Path;
	char *VirtualPath;
	char *Proxy;
} HttpParamMd;
#pragma pack(pop)

#pragma pack(push,1)
typedef struct {
	char *HttpsCrt;
	char *HttpsKey;

	char *MapContextType;
	int Timeout;
} HttpGlobalInfo;
#pragma pack(pop)

// void SetKey(char *HttpsCrt, char *HttpsKey);
// void RemoveServer(int64_t id);
// void UpdateServer(int64_t id, HttpParamMd *param);
