import sqlite3
import pandas as pd
import numpy as np
from scipy import stats

# 데이터베이스에 연결
conn = sqlite3.connect('your_database.db')  # 데이터베이스 파일명을 넣어주세요
cursor = conn.cursor()

# 데이터 로드
query = "SELECT * FROM result"
df = pd.read_sql_query(query, conn)

# 아바타 1의 승리 횟수 및 전투력 계산
avatar1_stats = df.groupby('avatar1_name').agg(
    wins=('avatar1_win', 'sum'),
    total=('id', 'count'),
    avg_cp=('avatar1_cp', 'mean')
).reset_index()

# 승리 확률 계산
avatar1_stats['win_rate'] = avatar1_stats['wins'] / avatar1_stats['total']

# 신뢰구간 계산 (95%)
confidence_level = 0.95
avatar1_stats['conf_interval'] = avatar1_stats.apply(
    lambda x: stats.norm.interval(
        confidence_level, 
        loc=x['win_rate'], 
        scale=np.sqrt((x['win_rate'] * (1 - x['win_rate'])) / x['total'])
    ),
    axis=1
)

# 결과 출력
print(avatar1_stats[['avatar1_name', 'avg_cp', 'win_rate', 'conf_interval']])
