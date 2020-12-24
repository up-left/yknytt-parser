from bs4 import BeautifulSoup
import os
import csv
import urllib.parse

writer = csv.writer(open('../knyttlevels.csv', 'w', newline=''))
    
for filename in sorted(os.listdir('../pages')):
    with open(f'../pages/{filename}') as f:
        print(f'Processing {filename}')
        soup = BeautifulSoup (f, 'html.parser')
        trs = soup.table.find_all('tr')
        for tr in trs[1:]:
            tds = tr.find_all('td')
            icon = tds[0].img['src']
            author = tds[1].text
            title = tds[2].text
            size = tds[3].text
            pub_date = tds[4].span['title']
            link = tds[5].a['href']
            fname = urllib.parse.unquote(link[link.rfind('/')+1:])
            downloaded = 'downloaded' if os.path.isfile('../worlds/' + fname) else 'n/a'
            writer.writerow([downloaded, title, author, size, pub_date, icon, link])
