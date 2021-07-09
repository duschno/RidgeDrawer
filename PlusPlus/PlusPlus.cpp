// PlusPlus.cpp : Defines the exported functions for the DLL.
#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include <cairo.h>
#include <cairo-win32.h>
#include <utility>
#include <limits.h>
#define _USE_MATH_DEFINES
#include <math.h>
#include "PlusPlus.h"

void simple_draw(HDC hdc)
{
	cairo_surface_t* surface = cairo_win32_surface_create(hdc);
	cairo_t* cr = cairo_create(surface);

	cairo_set_antialias(cr, CAIRO_ANTIALIAS_NONE);
	cairo_set_source_rgb(cr, 1, 1, 1);
	cairo_paint(cr);
	cairo_set_source_rgb(cr, 1, 0, 0);
	cairo_move_to(cr, 20, 20);
	cairo_line_to(cr, 380, 380);
	cairo_set_line_width(cr, 5);
	cairo_stroke(cr);

	cairo_destroy(cr);
	cairo_surface_destroy(surface);
}

void Draw(HDC hdc, int width, int height, int linesCount, int strokeWidth, int factor, int chunkSize)
{
	simple_draw(hdc);
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
