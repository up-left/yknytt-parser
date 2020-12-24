import csv
import urllib.parse

reader = csv.reader(open('../knyttlevels.csv'))

size = 0
for row in reader:
    if row[0] not in ('downloaded', 'n/a'):
        link = row[6]
        fname = urllib.parse.unquote(link[link.rfind('/')+1:])
        print(f'curl -o "../worlds/{fname}" http://knyttlevels.com/{link}')
        size += float(row[3][:-3]) * {'MB': 1024*1024, 'kB': 1024}[row[3][-2:]]
        
print(f'# {int(size/(1024*1024))} MB total')
