#include <stdio.h>
#include <math.h>
#include <float.h>
#include <omp.h>

#define PI 3.14159265358979323846
#define RUNS 3

double calc_pi(long long n)
{
    double h = 1.0 / n;
    double sum = 0.0;
    for (long long i = 0; i < n; i++)
    {
        double x = (i + 0.5) * h;
        sum += 4.0 / (1.0 + x * x);
    }
    return sum * h;
}

double measure(long long n, double* pi)
{
    double time_min = DBL_MAX;
    for (int k = 0; k < RUNS; k++)
    {
        double t = omp_get_wtime();
        *pi = calc_pi(n);
        t = omp_get_wtime() - t;
        if (t < time_min) time_min = t;
    }
    return time_min;
}

void task1()
{
    long long n = 100000000;
    double pi;
    double time_min = measure(n, &pi);
    printf("n = %lld pi = %.15f error = %.3e time = %lg\n",
           n, pi, fabs(pi - PI), time_min);
}

void task4()
{
    printf("%12s %10s %12s %12s\n",
           "n", "time, s", "time/n, ns", "error");
    for (long long n = 1000000; n <= 1000000000; n *= 10)
    {
        double pi;
        double time_min = measure(n, &pi);
        printf("%12lld %10.4f %12.3f %12.3e\n", n, time_min,
               time_min / n * 1e9, fabs(pi - PI));
    }
}

int main()
{
    printf("Task 1\n");
    task1();
    printf("\nTask 4\n");
    task4();
    return 0;
}
