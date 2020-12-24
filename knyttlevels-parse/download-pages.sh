for i in {1..27}; do 
  curl -o ../pages/page$(printf "%02d" $i).html http://knyttlevels.com/?l=$i
done