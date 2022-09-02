#include "stdafx.h"

#if !defined(OS_NT)
#include <pthread.h>
#endif

#include "Lock.h"

ILockable::ILockable()
{
#ifdef _DEBUG
	pFirstLockDebugData = nullptr;
	pLastLockDebugData = nullptr;
#endif

	CriticalSectionInitialized = 0;

	activeLockCount = 0;
}

int ILockable::InitCritSection()
{
	if (!CriticalSectionInitialized)
	{
#if defined(OS_NT)
		InitializeCriticalSectionAndSpinCount(&CriticalSection, 0x00000400); 
#else
		pthread_mutexattr_t attr;
                pthread_mutexattr_init(&attr);
                pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
		pthread_mutex_init(&cs_mutex,&attr);
#endif
		CriticalSectionInitialized = 1;
	}
	return 1;
}

void ILockable::FreeCriticalSection()
{
	if (CriticalSectionInitialized)
	{
#if defined(OS_NT)
		DeleteCriticalSection(&CriticalSection);
#else
		pthread_mutex_destroy(&cs_mutex);
#endif
		CriticalSectionInitialized = 0;
	}
}
#ifdef _DEBUG
Lock::Lock(ILockable* it, const char *_file , int _line)
{
	It = it;

	if (!it->CriticalSectionInitialized)
 	{
		return;
	}
	
#ifdef OS_NT
	EnterCriticalSection(&(it->CriticalSection)); 
#else
	pthread_mutex_lock(&(it->cs_mutex));
#endif
	file =  _file;
	line = _line;

	pNextLockDebugData = NULL;
	pPrevLockDebugData = NULL;

	if(!it->pFirstLockDebugData)
	{
		// first object, initialize the list with this as the only element
		it->pFirstLockDebugData = this;
		it->pLastLockDebugData = this;
	}
	else
	{
		// add to the end of the list
		it->pLastLockDebugData->pNextLockDebugData = this;
		this->pPrevLockDebugData = it->pLastLockDebugData;
		it->pLastLockDebugData = this;
	}

	(it->activeLockCount)++;
}
#else
Lock::Lock(ILockable* it)
{
	It = it;

	if (!it->CriticalSectionInitialized)
 	{
		return;
	}
	
#ifdef OS_NT
	EnterCriticalSection(&(it->CriticalSection)); 
#else
	pthread_mutex_lock(&(it->cs_mutex));
#endif
	(it->activeLockCount)++;
}
#endif
Lock::~Lock(void)
{
	if (!It->CriticalSectionInitialized)
 	{
		return;
	}

#ifdef _DEBUG
	if ((It->pFirstLockDebugData == this) && (It->pLastLockDebugData == this))
	{
		// only object in the list, so NULL out the list head and tail pointers
		It->pFirstLockDebugData = NULL;
		It->pLastLockDebugData = NULL;
	}
	else if (It->pFirstLockDebugData == this)
	{
		// first object in list, set the head to the next object in the list
		It->pFirstLockDebugData = this->pNextLockDebugData;
		It->pFirstLockDebugData->pPrevLockDebugData = NULL;
	}
	else if (It->pLastLockDebugData == this)
	{
		// last object, set the tail to the pervious object in the list
		It->pLastLockDebugData = this->pPrevLockDebugData;
		It->pLastLockDebugData->pNextLockDebugData = NULL;
	}
	else if ((this->pNextLockDebugData != NULL) && (this->pPrevLockDebugData != NULL))
	{
		// in the middle of the list, so link the pointers for the previous 
		//  and next objects.
		this->pPrevLockDebugData->pNextLockDebugData = this->pNextLockDebugData;
		this->pNextLockDebugData->pPrevLockDebugData = this->pPrevLockDebugData;
	}
#endif

	(It->activeLockCount)--;

#ifdef OS_NT
	LeaveCriticalSection(&(It->CriticalSection));
#else
	pthread_mutex_unlock(&(It->cs_mutex));
#endif
}
