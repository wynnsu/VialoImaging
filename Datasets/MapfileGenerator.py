import os
abs_path = os.path.dirname(os.path.abspath(__file__))


def generate(hit_path, miss_path):
    hit_list = os.listdir(hit_path)
    hit_list[:] = [os.path.join(hit_path, x) for x in hit_list]
    miss_list = os.listdir(miss_path)
    miss_list[:] = [os.path.join(miss_path, x) for x in miss_list]
    train_list = [[x, 1] for x in hit_list[:round(len(
        hit_list) * 3 / 4)]] + [[y, 0] for y in miss_list[:round(len(miss_list) * 3 / 4)]]
    test_list = [[x, 1] for x in hit_list[round(len(
        hit_list) * 3 / 4):]] + [[y, 0] for y in miss_list[round(len(miss_list) * 3 / 4):]]
    train_out = os.path.join(abs_path, 'CIFAR-10', 'train_map.txt')
    test_out = os.path.join(abs_path, 'CIFAR-10', 'test_map.txt')
    if os.path.exists(train_out):
        os.remove(train_out)
    if os.path.exists(test_out):
        os.remove(test_out)
    with open(train_out, 'a') as f:
        for pair in train_list:
            f.write('{0}\t{1}\n'.format(pair[0], pair[1]))
    with open(test_out, 'a') as f:
        for pair in test_list:
            f.write('{0}\t{1}\n'.format(pair[0], pair[1]))


if __name__ == '__main__':
    generate(os.path.join(abs_path,  "1"), os.path.join(abs_path, "0"))
