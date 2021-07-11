// PlusPlus.h - Contains declarations of math functions
#pragma once

#define MATHLIBRARY_API __declspec(dllexport)

extern "C" MATHLIBRARY_API void Draw(HDC hdc, int* x, int* y, int size);

extern "C" MATHLIBRARY_API void SetLineType(int smoothingType);

extern "C" MATHLIBRARY_API void SetMethodType(int smoothingType);

extern "C" MATHLIBRARY_API void SetSmoothingType(int smoothingType);