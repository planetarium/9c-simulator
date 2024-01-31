import pandas as pd
import numpy as np
from scipy import stats
import sqlite3
import matplotlib.pyplot as plt

conn = sqlite3.connect('acc.db')
query = f"SELECT * FROM result"
df = pd.read_sql_query(query, conn)

sample_sizes = [10, 20, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000]
error_margins_dict = {}

for avatar_pair, group in df.groupby(['avatar1_address', 'avatar2_address']):
    error_margins = []
    for sample_size in sample_sizes:
        temp_df = group.head(sample_size)
        wins = temp_df['avatar1_win'].sum()
        total = len(temp_df)
        win_rate = (wins / total) * 100
        confidence_level = 0.95
        z_value = stats.norm.ppf((1 + confidence_level) / 2)

        conf_interval_low = win_rate - z_value * np.sqrt((win_rate / 100 * (1 - win_rate / 100)) / total) * 100
        conf_interval_high = win_rate + z_value * np.sqrt((win_rate / 100 * (1 - win_rate / 100)) / total) * 100

        error_margin = conf_interval_high - conf_interval_low

        error_margins.append(error_margin)
    error_margins_dict[avatar_pair] = error_margins

avg_error_margins = np.mean(list(error_margins_dict.values()), axis=0)
plt.figure(figsize=(10, 6))
plt.plot(sample_sizes, avg_error_margins, marker='o', linestyle='-', color='r')
plt.title('All Avatars')
plt.xlabel('Sample Size')
plt.ylabel('Average Error Margin (%)')
plt.grid(True)
plt.show()

max_error_margin = max(error_margins_dict.items(), key=lambda x: x[1][-1])

plt.figure(figsize=(10, 6))
plt.plot(sample_sizes, max_error_margin[1], marker='o', linestyle='-', color='g')
plt.title(f'Max {max_error_margin[0]}')
plt.xlabel('Sample Size')
plt.ylabel('Error Margin (%)')
plt.grid(True)
plt.show()
