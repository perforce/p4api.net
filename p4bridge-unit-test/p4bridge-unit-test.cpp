// p4bridge-unit-test.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "UnitTestFrameWork.h"

#include <iostream>
#include <cstring>

int main(int argc, char* argv[])
{
    UnitTestFrameWork *frame = new UnitTestFrameWork();

    if (argc > 0){
        for (int idx = 1; idx < argc; idx++)
        {
            if (strcmp(argv[idx],"-s") == 0) // Set source directory (used to find tarballs)
            {
                UnitTestFrameWork::SetSourceDirectory(argv[++idx]);
            }
            else if (strcmp(argv[idx], "-b") == 0) // break on fail
                UnitTestSuite::BreakOnFailure(true); 
            else if (strcmp(argv[idx], "-e") == 0) // end on fail
                UnitTestSuite::EndOnFailure(true); 
            else
            {
			// assume this is a test name to match (don't run tests that do not match)
			UnitTestFrameWork::AddTestMatch(argv[idx]);
        }
        }
    } 
    
    UnitTestFrameWork::RunTests();
#ifdef _DEBUG_MEMORY
	p4base::PrintMemoryState("After test complete");
	p4base::DumpMemoryState("After test complete");
#endif
    printf("Hit enter to exit");
    // std::cin.ignore();

    delete frame;
    return 0;
}

