#include <windows.h>
#include <intrin.h>
#include <omp.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <float.h>
#include <limits.h>

#define N 1024
#define M 100

int a[N][N], b[N][N], c[N][N];
int a2[M][M], b2[M][M], c2[M][M];

template <int S>
void mul_ijk(int c[][S], int a[][S], int b[][S])
{
    int i, j, k;
    memset(c, 0, S * S * sizeof(int));
    for (i = 0; i < S; i++)
        for (j = 0; j < S; j++)
            for (k = 0; k < S; k++)
                c[i][j] += a[i][k] * b[k][j];
}

template <int S>
void mul_ikj(int c[][S], int a[][S], int b[][S])
{
    int i, j, k;
    memset(c, 0, S * S * sizeof(int));
    for (i = 0; i < S; i++)
        for (k = 0; k < S; k++)
            for (j = 0; j < S; j++)
                c[i][j] += a[i][k] * b[k][j];
}

template <int S>
void mul_jik(int c[][S], int a[][S], int b[][S])
{
    int i, j, k;
    memset(c, 0, S * S * sizeof(int));
    for (j = 0; j < S; j++)
        for (i = 0; i < S; i++)
            for (k = 0; k < S; k++)
                c[i][j] += a[i][k] * b[k][j];
}

template <int S>
void mul_jki(int c[][S], int a[][S], int b[][S])
{
    int i, j, k;
    memset(c, 0, S * S * sizeof(int));
    for (j = 0; j < S; j++)
        for (k = 0; k < S; k++)
            for (i = 0; i < S; i++)
                c[i][j] += a[i][k] * b[k][j];
}

template <int S>
void mul_kij(int c[][S], int a[][S], int b[][S])
{
    int i, j, k;
    memset(c, 0, S * S * sizeof(int));
    for (k = 0; k < S; k++)
        for (i = 0; i < S; i++)
            for (j = 0; j < S; j++)
                c[i][j] += a[i][k] * b[k][j];
}

template <int S>
void mul_kji(int c[][S], int a[][S], int b[][S])
{
    int i, j, k;
    memset(c, 0, S * S * sizeof(int));
    for (k = 0; k < S; k++)
        for (j = 0; j < S; j++)
            for (i = 0; i < S; i++)
                c[i][j] += a[i][k] * b[k][j];
}

typedef void (*MulFun)(int[][N], int[][N], int[][N]);

void measure(const char* name, MulFun mul)
{
    // Визначення часу
    double time_min, time_cur;
    time_min = FLT_MAX;
    for (int i = 0; i < 5; i++)
    {
        time_cur = omp_get_wtime();
        mul(c, a, b);
        time_cur = omp_get_wtime() - time_cur;
        if (time_cur < time_min)
            time_min = time_cur;
    }
    printf("%s : c[0][0] = %d c[N-1][N-1] = %d time = %lg\n",
        name, c[0][0], c[N - 1][N - 1], time_min);
}

void task3()
{
    // Ініціалізація матриць
    for (int i = 0; i < N; i++)
        for (int j = 0; j < N; j++)
        {
            a[i][j] = rand() % 5;
            b[i][j] = rand() % 5;
        }

    measure("mul_ijk", mul_ijk<N>);
    measure("mul_ikj", mul_ikj<N>);
    measure("mul_jik", mul_jik<N>);
    measure("mul_jki", mul_jki<N>);
    measure("mul_kij", mul_kij<N>);
    measure("mul_kji", mul_kji<N>);
}

void task5()
{
    int r[4];
    unsigned __int64 ll_cur, ll_min, ll_empty;
    double time_cur = 0;
    DWORD tick_cur = 0;

    ll_min = ULLONG_MAX;
    for (int i = 0; i < 500; i++)
    {
        __cpuid(r, 0);
        ll_cur = __rdtsc();
        __cpuid(r, 0);
        ll_cur = __rdtsc() - ll_cur;
        if (ll_cur < ll_min)
            ll_min = ll_cur;
    }
    ll_empty = ll_min;
    printf("empty         : time = %I64u\n", ll_empty);

    ll_min = ULLONG_MAX;
    for (int i = 0; i < 500; i++)
    {
        __cpuid(r, 0);
        ll_cur = __rdtsc();
        time_cur += omp_get_wtime();
        __cpuid(r, 0);
        ll_cur = __rdtsc() - ll_cur;
        if (ll_cur < ll_min)
            ll_min = ll_cur;
    }
    printf("omp_get_wtime : time_cur = %lg time = %I64u net = %I64u\n",
        time_cur, ll_min, ll_min - ll_empty);

    ll_min = ULLONG_MAX;
    for (int i = 0; i < 500; i++)
    {
        __cpuid(r, 0);
        ll_cur = __rdtsc();
        tick_cur += GetTickCount();
        __cpuid(r, 0);
        ll_cur = __rdtsc() - ll_cur;
        if (ll_cur < ll_min)
            ll_min = ll_cur;
    }
    printf("GetTickCount  : tick_cur = %lu time = %I64u net = %I64u\n",
        tick_cur, ll_min, ll_min - ll_empty);
}

typedef void (*MulFun2)(int[][M], int[][M], int[][M]);

void measure_rel(const char* name, MulFun2 mul)
{
    int count = 0;
    DWORD tick_cur = GetTickCount();
    while (GetTickCount() - tick_cur < 2000)
    {
        count++;
        mul(c2, a2, b2);
    }

    double time_min = FLT_MAX, time_cur;
    for (int i = 0; i < 5; i++)
    {
        time_cur = omp_get_wtime();
        mul(c2, a2, b2);
        time_cur = omp_get_wtime() - time_cur;
        if (time_cur < time_min)
            time_min = time_cur;
    }

    printf("%s : c[0][0] = %d c[99][99] = %d count = %d "
        "t_rel = %.4f ms time = %.4f ms\n",
        name, c2[0][0], c2[M - 1][M - 1], count,
        2000.0 / count, time_min * 1000);
}

void task6()
{
    // Ініціалізація матриць
    for (int i = 0; i < M; i++)
        for (int j = 0; j < M; j++)
        {
            a2[i][j] = rand() % 5;
            b2[i][j] = rand() % 5;
        }

    measure_rel("mul_ijk", mul_ijk<M>);
    measure_rel("mul_ikj", mul_ikj<M>);
    measure_rel("mul_jik", mul_jik<M>);
    measure_rel("mul_jki", mul_jki<M>);
    measure_rel("mul_kij", mul_kij<M>);
    measure_rel("mul_kji", mul_kji<M>);
}

int main()
{
    int choice = 0;
    printf("1 - task 3, 2 - task 5, 3 - task 6: ");
    scanf_s("%d", &choice);

    if (choice == 1) task3();
    if (choice == 2) task5();
    if (choice == 3) task6();
    return 0;
}