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

	double split = 0.5;
	//cairo_pattern_set_filter(cairo_get_source(cr), CAIRO_FILTER_NEAREST);
	//cairo_set_line_join(cr, CAIRO_LINE_JOIN_BEVEL);
	//cairo_set_line_cap(cr, CAIRO_LINE_CAP_SQUARE);
	cairo_set_antialias(cr, CAIRO_ANTIALIAS_NONE);
	cairo_set_source_rgb(cr, 1, 0, 0);
	for (int i = 0; i < size - 2; i++)
	{
		cairo_move_to(cr, *(x + i) + split, *(y + i) + split);
		cairo_line_to(cr, *(x + i + 1) + split, *(y + i + 1) + split);
	}
	cairo_set_line_width(cr, 1);
	cairo_stroke(cr);
	//cairo_surface_write_to_png(surface, "spiral.png");
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
