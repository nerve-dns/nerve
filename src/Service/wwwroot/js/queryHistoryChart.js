window.queryHistoryChart = {
    chart: null,
    create: (canvas, labels, valuesForwarded, valuesCached, valuesBlocked, title) => {
        const ctx = canvas.getContext('2d');

        window.queryHistoryChart.chart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: "Forwarded",
                        data: valuesForwarded,
                        backgroundColor: 'rgba(30, 232, 40, 0.5)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    },
                    {
                        label: "Cached",
                        data: valuesCached,
                        backgroundColor: 'rgba(32, 136, 234, 0.63)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    },
                    {
                        label: "Blocked",
                        data: valuesBlocked,
                        backgroundColor: 'rgba(235, 43, 43, 0.63)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: { stacked: true },
                    y: { stacked: true }
                }
            }
        });
    },
    update: (labels, valuesForwarded, valuesCached, valuesBlocked) => {
        if (!window.queryHistoryChart.chart) {
            return;
        }

        window.queryHistoryChart.chart.data.labels = labels;
        window.queryHistoryChart.chart.data.datasets[0].data = valuesForwarded;
        window.queryHistoryChart.chart.data.datasets[1].data = valuesCached;
        window.queryHistoryChart.chart.data.datasets[2].data = valuesBlocked;
        window.queryHistoryChart.chart.update();
    }
};
