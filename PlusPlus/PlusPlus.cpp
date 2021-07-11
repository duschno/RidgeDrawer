// PlusPlus.cpp : Defines the exported functions for the DLL.
#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include <cairo.h>
#include <cairo-win32.h>
#include <utility>
#include <limits.h>
#define _USE_MATH_DEFINES
#include <math.h>
#include "PlusPlus.h"

void Draw(HDC hdc, int* x, int* y, int size)
{
	cairo_surface_t* surface = cairo_win32_surface_create(hdc);
	cairo_t* cr = cairo_create(surface);

	cairo_set_antialias(cr, CAIRO_ANTIALIAS_NONE);
	cairo_set_source_rgb(cr, 1, 0, 0);
	for (int i = 0; i < size - 2; i++)
	{
		cairo_move_to(cr, *(x + i) + 0.5, *(y + i) + 0.5);
		cairo_line_to(cr, *(x + i + 1) + 0.5, *(y + i + 1) + 0.5);
	}
	cairo_set_line_width(cr, 1);
	cairo_stroke(cr);

	cairo_destroy(cr);
	cairo_surface_destroy(surface);
}

void SetLineType(int smoothingType)
{
}

void SetMethodType(int smoothingType)
{
}

void SetSmoothingType(int smoothingType)
{
}
