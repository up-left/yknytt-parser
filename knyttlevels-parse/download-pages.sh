for i in {1..29}; do 
  curl -o ../pages/page$(printf "%02d" $i).html https://knyttlevels.com/?l=$i
done