#include <stdio.h>
#include <stdlib.h>
#include <omp.h>
#include <ppl.h>
#include <vector>
#include <algorithm>

using namespace concurrency;

struct MYCOMPLEX {
    int re, im;
public:
    MYCOMPLEX(int x1, int y1) : re(x1), im(y1) {}
    MYCOMPLEX() { re = 0; im = 0; }
};

template <class Function>
double time_call(Function&& f)
{
    double begin = omp_get_wtime();
    f();
    return omp_get_wtime() - begin;
}

bool cmp(std::vector<MYCOMPLEX>& a, std::vector<MYCOMPLEX>& b)
{
    bool bRes = a.size() == b.size();
    if (bRes) {
        for (size_t i = 0; i < a.size(); i++) {
            int temp1 = a[i].re, temp2 = b[i].re;
            int temp3 = a[i].im, temp4 = b[i].im;
            int temp5 = temp1 * temp1 + temp3 * temp3;
            int temp6 = temp2 * temp2 + temp4 * temp4;
            bRes = temp5 == temp6;
            if (!bRes) break;
        }
    }
    return bRes;
}

void sort_test(int N)
{
    printf("N = %d\n", N);
    std::vector<MYCOMPLEX> a(N), b(N), c(N);
    for (int i = 0; i < N; i++)
        a[i] = MYCOMPLEX(rand() % 2001 - 1000,
            rand() % 2001 - 1000);

    std::copy(a.begin(), a.end(), b.begin());
    double elapsed = time_call([&] {
        std::sort(b.begin(), b.end(),
            [](MYCOMPLEX& x, MYCOMPLEX& y) {
                int d1 = (x.re * x.re + x.im * x.im);
                int d2 = (y.re * y.re + y.im * y.im);
                return d1 < d2;
            });
        });
    printf("std::sort time = %lg\n", elapsed);

    std::copy(a.begin(), a.end(), c.begin());
    elapsed = time_call([&] {
        parallel_sort(c.begin(), c.end(),
            [](MYCOMPLEX& x, MYCOMPLEX& y) {
                int d1 = (x.re * x.re + x.im * x.im);
                int d2 = (y.re * y.re + y.im * y.im);
                return d1 < d2;
            });
        });
    printf("parallel_sort time = %lg\n", elapsed);
    printf("%s\n", cmp(b, c) ? "Yes" : "No");

    std::copy(a.begin(), a.end(), c.begin());
    elapsed = time_call([&] {
        parallel_buffered_sort(c.begin(), c.end(),
            [](MYCOMPLEX& x, MYCOMPLEX& y) {
                int d1 = (x.re * x.re + x.im * x.im);
                int d2 = (y.re * y.re + y.im * y.im);
                return d1 < d2;
            });
        });
    printf("parallel_buffered_sort time = %lg\n", elapsed);
    printf("%s\n", cmp(b, c) ? "Yes" : "No");
    printf("\n");
}

int main()
{
    for (int N = 1000; N <= 100000; N *= 10)
        sort_test(N);
    return 0;
}