#pragma once

typedef void (CALLBACK* LoggerFunc)(const wchar_t*);

inline LoggerFunc logger;
