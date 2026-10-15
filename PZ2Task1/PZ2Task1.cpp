#include <windows.h>
#include <tchar.h>
#include <intrin.h>
#include <immintrin.h>
#include <stdio.h>
#include <string.h>

typedef enum {
    INTEL, AMD, OTHER
} ProcessorType;

ProcessorType DefineProcessorType()
{
    ProcessorType Type = OTHER;
    CHAR INTELName[13] = "GenuineIntel";
    CHAR AMDName[13] = "AuthenticAMD";
    CHAR ProcessorName[13];
    ProcessorName[12] = 0;
    INT Regs[4];
    __cpuid(Regs, 0);
    memcpy(ProcessorName, &Regs[1], 4);
    memcpy(ProcessorName + 4, &Regs[3], 4);
    memcpy(ProcessorName + 8, &Regs[2], 4);
    if (memcmp(INTELName, ProcessorName, 12) == 0)
        Type = INTEL;
    else
    {
        if (memcmp(AMDName, ProcessorName, 12) == 0)
            Type = AMD;
    }
    return Type;
}

void task1()
{
    ProcessorType Type;
    Type = DefineProcessorType();
    switch (Type)
    {
    case INTEL:
        _tprintf(TEXT("ProcessorType: INTEL\n")); break;
    case AMD:
        _tprintf(TEXT("ProcessorType: AMD\n")); break;
    case OTHER:
        _tprintf(TEXT("ProcessorType: OTHER\n")); break;
    }
}

typedef enum {
    SSESUPPORT, SSE2SUPPORT, SSE3SUPPORT, SSSE3SUPPORT,
    SSE41SUPPORT, SSE42SUPPORT, AVXSUPPORT, AVX2SUPPORT,
    AVX512SUPPORT
} SIMDType;

unsigned MaxFun(unsigned fun)
{
    int Regs[4];
    __cpuid(Regs, fun);
    return Regs[0];
}

bool check_properties(int fun, int reg, int bit)
{
    int Regs[4];
    __cpuidex(Regs, fun, 0);
    return (((unsigned)Regs[reg] >> bit) & 1) != 0;
}

unsigned SIMDSupport()
{
    bool AVX512, AVX, SSE, b;
    unsigned maska = 0;
    unsigned max_fun = MaxFun(0);
    // Перевірка можливості використання команд XSAVE, XRESTOR, XGETBV:
    if (max_fun >= 1) {
        b = check_properties(1, 2, 26) && check_properties(1, 2, 27);
        if (b) {
            unsigned long long res = _xgetbv(0);
            int flags1 = 7 + (7 << 5), flags2 = 7, flags3 = 3;
            AVX512 = (res & flags1) == flags1;
            AVX = (res & flags2) == flags2;
            SSE = (res & flags3) == flags3;
            if (max_fun >= 7 && AVX512) {
                // AVX512
                b = check_properties(7, 1, 16);
                if (b) maska |= (1 << AVX512SUPPORT);
            }
            if (AVX) {
                if (max_fun >= 7) {
                    // AVX2
                    b = check_properties(7, 1, 5);
                    if (b) maska |= 1 << AVX2SUPPORT;
                }
                if (max_fun >= 1) {
                    // AVX
                    b = check_properties(1, 2, 28);
                    if (b) maska |= 1 << AVXSUPPORT;
                }
            }
            if (SSE) {
                b = check_properties(1, 2, 20);
                if (b) maska |= 1 << SSE42SUPPORT;
                b = check_properties(1, 2, 19);
                if (b) maska |= 1 << SSE41SUPPORT;
                b = check_properties(1, 2, 9);
                if (b) maska |= 1 << SSSE3SUPPORT;
                b = check_properties(1, 2, 0);
                if (b) maska |= 1 << SSE3SUPPORT;
                b = check_properties(1, 3, 26);
                if (b) maska |= 1 << SSE2SUPPORT;
                b = check_properties(1, 3, 25);
                if (b) maska |= 1 << SSESUPPORT;
            }
        }
    }
    return maska;
}

void task5()
{
    const char* names[] = { "SSE", "SSE2", "SSE3", "SSSE3", "SSE4.1",
                            "SSE4.2", "AVX", "AVX2", "AVX512" };
    unsigned maska = SIMDSupport();
    printf("SIMD mask = 0x%03X\n", maska);
    for (int i = SSESUPPORT; i <= AVX512SUPPORT; i++)
        printf("%-7s : %s\n", names[i],
            (maska & (1 << i)) ? "yes" : "no");
}

int main()
{
    printf("Task 1\n");
    task1();
    printf("\nTask 5\n");
    task5();
    return 0;
}