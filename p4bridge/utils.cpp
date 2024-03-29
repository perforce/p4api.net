/*******************************************************************************

Copyright (c) 2010, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: utils.cpp
 *
 * Author	: dbb
 *
 * Description	: Utilities to copy and duplicate strings to be returned by
 *  the P4 Bridge API.
 *
 ******************************************************************************/
#include "stdafx.h"

#include "P4BridgeServer.h"

#include <set>

#ifndef OS_NT
#if defined(_DEBUG)

#endif

#include <atomic>
#define __STDC_WANT_LIB_EXT1__ 1  // for strcpy_s
#endif


namespace Utils
{
#if defined(_DEBUG)

#ifdef OS_NT
	volatile long allocs = 0;
	volatile long frees = 0;

#define ALLOC_COUNT(p)	{ InterlockedIncrement(&allocs); }
#define FREE_COUNT(p)	{ InterlockedIncrement(&frees); }
	long AllocCount() { return allocs; }
	long FreeCount() { return frees; }
#else // Linux or OSX
    std::atomic<long> allocs{0};
    std::atomic<long> frees{0};

#define ALLOC_COUNT(p) { allocs.fetch_add(1, std::memory_order_relaxed); }
#define FREE_COUNT(p)  { frees.fetch_add(1, std::memory_order_relaxed); }
    long AllocCount() { return allocs.load(); }
    long FreeCount() { return frees.load(); }
#endif

#else // Not _DEBUG
#define ALLOC_COUNT(p) {  }
#define FREE_COUNT(p) {  }

	long AllocCount() { return 0; }
	long FreeCount() { return 0; }
#endif

	const char* AllocString(const string& s)
	{
		return AllocString(s.c_str());
	}

	const char* AllocString(const char* p)
	{
		// return NULL, some of the p4.net-api code -> p4bridge code relies on NULL 
		// re: debug tracking, don't bother tracking NULL allocs, that's not useful
		if (p == NULL) return NULL;
		size_t len = strlen(p);
		if (len == 0) return NULL;
		char* ret = new char[len + 1];
		LOG_DEBUG3(4, "Alloc [%d]: (%p) %s", AllocCount(), ret, p);
		ALLOC_COUNT(ret);
		strncpy(ret, p, len);
		ret[len] = '\0';      // null terminate
		return ret;
	}

	void ReleaseString(const char* s)
	{
		ReleaseString((void*)s);
	}

	void ReleaseString(void* p)
	{
		if (!p) return;	// skip the debug logging code
		LOG_DEBUG3(4, "Free [%d]: (%p) %s", FreeCount(), p, p);
		FREE_COUNT((const char*)p);
		delete[] (const char *)p;
	}

	string stringFromPtr(const char* s)
	{
		return (s == NULL) ? string() : s;
	}
}