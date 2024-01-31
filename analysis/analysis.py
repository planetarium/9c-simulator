import pandas as pd
import numpy as np
from scipy import stats
import sqlite3

print("Avatar Address: ")

avatar_address = input()
print("Limit: ")

limit = int(input())

conn = sqlite3.connect('acc.db')
query = f"SELECT * FROM result WHERE avatar1_address = '{avatar_address}'"

df = pd.read_sql_query(query, conn)

df = df.groupby(['avatar1_address', 'avatar2_address']).head(limit)

pairwise_stats = df.groupby(['avatar1_address', 'avatar2_address', 'avatar1_name', 'avatar2_name']).agg(
    wins=('avatar1_win', 'sum'),
    total=('id', 'count')
).reset_index()

pairwise_stats['win_rate'] = pairwise_stats['wins'] / pairwise_stats['total'] * 100

confidence_level = 0.95
z_value = stats.norm.ppf((1 + confidence_level) / 2)
pairwise_stats['conf_interval_low'] = pairwise_stats['win_rate'] - z_value * np.sqrt((pairwise_stats['win_rate'] / 100 * (1 - pairwise_stats['win_rate'] / 100)) / pairwise_stats['total']) * 100
pairwise_stats['conf_interval_high'] = pairwise_stats['win_rate'] + z_value * np.sqrt((pairwise_stats['win_rate'] / 100 * (1 - pairwise_stats['win_rate'] / 100)) / pairwise_stats['total']) * 100

pairwise_stats['error_margin'] = pairwise_stats['conf_interval_high'] - pairwise_stats['conf_interval_low']

print(pairwise_stats[['avatar1_address', 'avatar1_name', 'avatar2_address', 'avatar2_name', 'win_rate', 'conf_interval_low', 'conf_interval_high', 'error_margin']])
