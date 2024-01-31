import pandas as pd
import sqlite3

conn = sqlite3.connect('acc.db')
query = "SELECT * FROM result"
df = pd.read_sql_query(query, conn)

df['win_rate'] = df['avatar1_win'] / df.groupby(['avatar1_address', 'avatar2_address'])['id'].transform('count')

avg_win_rates = df.groupby(['avatar1_address', 'avatar1_name'])['win_rate'].mean().reset_index()
avg_win_rates.columns = ['avatar_address', 'avatar_name', 'avg_win_rate']

avg_win_rates['avg_win_rate'] = avg_win_rates['avg_win_rate'] * 100

avg_win_rates = avg_win_rates.sort_values(by='avg_win_rate', ascending=False).reset_index(drop=True)

avg_win_rates['rank'] = avg_win_rates.index + 1

print(avg_win_rates)
