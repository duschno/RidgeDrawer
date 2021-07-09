// PlusPlus.h - Contains declarations of math functions
#pragma once

#define MATHLIBRARY_API __declspec(dllexport)

// simple_draw
extern "C" MATHLIBRARY_API void simple_draw(HDC hdc);

extern "C" MATHLIBRARY_API void Draw(HDC hdc, int width, int height, int linesCount, int strokeWidth, int factor, int chunkSize);

extern "C" MATHLIBRARY_API void SetLineType(int smoothingType);

extern "C" MATHLIBRARY_API void SetMethodType(int smoothingType);

extern "C" MATHLIBRARY_API void SetSmoothingType(int smoothingType);